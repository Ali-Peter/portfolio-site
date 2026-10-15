using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Portfolio.Application.Common;
using Portfolio.Application.Persistence;
using Portfolio.Domain.Entities;

namespace Portfolio.Application.Services;

/// <summary>
/// Rebuilds the RAG index from source entities: profile overview, projects,
/// published blog posts, skill categories and experience entries. The whole
/// index is replaced per run (portfolio-scale: a few dozen rows), embeddings
/// are batched to keep provider calls low.
/// </summary>
public sealed class ContentIndexer : IPortfolioIndexer
{
    private const int EmbeddingBatchSize = 64;
    private const string IndexerUser = "system:ai-index";

    private readonly IPortfolioDbContext _db;
    private readonly IEmbeddingService _embeddings;
    private readonly ILogger<ContentIndexer> _logger;

    public ContentIndexer(
        IPortfolioDbContext db,
        IEmbeddingService embeddings,
        ILogger<ContentIndexer> logger)
    {
        _db = db;
        _embeddings = embeddings;
        _logger = logger;
    }

    public async Task<int> ReindexAsync(CancellationToken cancellationToken = default)
    {
        if (!_embeddings.IsConfigured)
        {
            _logger.LogInformation("AI indexing skipped: no embedding provider configured.");
            return 0;
        }

        var drafts = new List<ContentDraft>();
        drafts.AddRange(await BuildProjectChunksAsync(cancellationToken));
        drafts.AddRange(await BuildBlogChunksAsync(cancellationToken));
        drafts.AddRange(await BuildSkillChunksAsync(cancellationToken));
        drafts.AddRange(await BuildExperienceChunksAsync(cancellationToken));
        drafts.AddRange(await BuildProfileChunksAsync(cancellationToken));

        if (drafts.Count == 0)
        {
            _logger.LogWarning("AI indexing found no portfolio content.");
            return 0;
        }

        // Embed in batches before touching the database so a provider failure
        // leaves the previous index intact.
        var embeddings = new List<float[]>(drafts.Count);
        for (var offset = 0; offset < drafts.Count; offset += EmbeddingBatchSize)
        {
            var batch = drafts.Skip(offset).Take(EmbeddingBatchSize).Select(d => d.Text).ToList();
            var vectors = await _embeddings.EmbedBatchAsync(batch, cancellationToken);
            embeddings.AddRange(vectors);
        }

        await _db.ContentChunks.ExecuteDeleteAsync(cancellationToken);

        foreach (var (draft, vector) in drafts.Zip(embeddings))
        {
            _db.ContentChunks.Add(new ContentChunk
            {
                SourceType = draft.SourceType,
                SourceId = draft.SourceId,
                SourceSlug = draft.SourceSlug,
                Title = draft.Title,
                Content = draft.Text,
                Embedding = new Pgvector.Vector(vector),
                CreatedBy = IndexerUser,
            });
        }

        await _db.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("AI index rebuilt: {Count} chunks.", drafts.Count);
        return drafts.Count;
    }

    /// <summary>One header chunk per project plus description paragraph chunks.</summary>
    private async Task<List<ContentDraft>> BuildProjectChunksAsync(CancellationToken cancellationToken)
    {
        var drafts = new List<ContentDraft>();
        var projects = await _db.Projects.AsNoTracking().OrderBy(p => p.SortOrder).ToListAsync(cancellationToken);

        foreach (var project in projects)
        {
            var header =
                $"{project.Title} — portfolio project ({FormatEnum(project.Status)}). " +
                $"{project.Summary} Technologies: {string.Join(", ", project.Technologies)}. " +
                FormatProjectLinks(project);
            drafts.Add(ContentDraft.ForProject(project, header.Trim()));

            foreach (var paragraph in TextChunker.SplitToChunks(project.Description))
            {
                drafts.Add(ContentDraft.ForProject(project, paragraph));
            }
        }

        return drafts;
    }

    /// <summary>Published blog posts only: intro chunk + body chunks (Markdown cleaned).</summary>
    private async Task<List<ContentDraft>> BuildBlogChunksAsync(CancellationToken cancellationToken)
    {
        var drafts = new List<ContentDraft>();
        var posts = await _db.BlogPosts.AsNoTracking()
            .Where(p => p.IsPublished)
            .OrderByDescending(p => p.PublishedAt)
            .ToListAsync(cancellationToken);

        foreach (var post in posts)
        {
            var published = post.PublishedAt?.ToString("yyyy-MM-dd") ?? "undated";
            var intro =
                $"Blog article: {post.Title} (published {published}, {post.ReadingMinutes} min read). " +
                $"{post.Excerpt} Tags: {string.Join(", ", post.Tags)}.";
            drafts.Add(ContentDraft.ForBlogPost(post, intro));

            var body = TextChunker.StripMarkdownDecorations(post.BodyMarkdown);
            foreach (var paragraph in TextChunker.SplitToChunks(body))
            {
                drafts.Add(ContentDraft.ForBlogPost(post, paragraph));
            }
        }

        return drafts;
    }

    /// <summary>One chunk per skill category listing skills and proficiency levels.</summary>
    private async Task<List<ContentDraft>> BuildSkillChunksAsync(CancellationToken cancellationToken)
    {
        var skills = await _db.Skills.AsNoTracking().OrderBy(s => s.SortOrder).ToListAsync(cancellationToken);

        return skills
            .GroupBy(s => s.Category)
            .OrderBy(g => g.Key)
            .Select(group =>
            {
                var list = string.Join(", ", group.Select(s => $"{s.Name} ({FormatEnum(s.Proficiency)})"));
                return ContentDraft.ForCategory(
                    FormatEnum(group.Key),
                    $"Skills in {FormatEnum(group.Key)}: {list}.");
            })
            .ToList();
    }

    /// <summary>One chunk per experience entry.</summary>
    private async Task<List<ContentDraft>> BuildExperienceChunksAsync(CancellationToken cancellationToken)
    {
        var experiences = await _db.Experiences.AsNoTracking()
            .OrderBy(e => e.SortOrder)
            .ToListAsync(cancellationToken);

        return experiences
            .Select(experience => ContentDraft.ForExperience(
                experience,
                $"{experience.Title} at {experience.Company} ({FormatPeriod(experience.StartedOn, experience.EndedOn, experience.IsCurrent)}). " +
                $"{experience.Description} Technologies: {string.Join(", ", experience.Technologies)}."))
            .ToList();
    }

    /// <summary>
    /// Profile overview grounded in real site content (tagline mirrors the
    /// footer, current role and skills come from the database).
    /// </summary>
    private async Task<List<ContentDraft>> BuildProfileChunksAsync(CancellationToken cancellationToken)
    {
        var currentRoles = await _db.Experiences.AsNoTracking()
            .Where(e => e.IsCurrent)
            .OrderBy(e => e.SortOrder)
            .ToListAsync(cancellationToken);
        var skills = await _db.Skills.AsNoTracking().OrderBy(s => s.SortOrder).ToListAsync(cancellationToken);

        var currentRole = currentRoles.Count > 0
            ? $"Currently {currentRoles[0].Title} at {currentRoles[0].Company}."
            : string.Empty;
        var skillHighlights = string.Join(", ",
            skills.Where(s => s.Proficiency is Domain.Enums.SkillProficiency.Expert
                or Domain.Enums.SkillProficiency.Advanced).Select(s => s.Name).Distinct());

        var profile =
            "Ali Ogochukwu Peter is a Software & AI engineer building typed backends, " +
            "modern frontends and applied LLM systems. " + currentRole +
            $" Core strengths include: {skillHighlights}. " +
            "Portfolio highlights include an enterprise ERP (GlobalSuite ERP), an AI call " +
            "automation backend (Outbound AI Backend), a full-stack learning platform " +
            "(DevAtlas Academy) and an Angular + ASP.NET Core portfolio site with a RAG " +
            "AI assistant. Contact: aliogochukwu06@gmail.com. GitHub: github.com/Ali-Peter.";

        return [ContentDraft.ForProfile("Profile overview", profile)];
    }

    private static string FormatProjectLinks(Project project)
    {
        var links = new List<string>();
        if (!string.IsNullOrWhiteSpace(project.RepoUrl))
        {
            links.Add($"Repository: {project.RepoUrl}");
        }

        if (!string.IsNullOrWhiteSpace(project.DemoUrl))
        {
            links.Add($"Live demo: {project.DemoUrl}");
        }

        return links.Count > 0 ? string.Join(" ", links) : string.Empty;
    }

    private static string FormatPeriod(DateOnly? startedOn, DateOnly? endedOn, bool isCurrent)
    {
        var start = startedOn?.ToString("yyyy-MM") ?? "unknown start";
        if (isCurrent)
        {
            return $"{start} – present";
        }

        var end = endedOn?.ToString("yyyy-MM") ?? "unknown end";
        return $"{start} – {end}";
    }

    private static string FormatEnum(Enum value) => value.ToString();

    /// <summary>Chunk text plus the source metadata persisted alongside it.</summary>
    private sealed record ContentDraft(
        string SourceType,
        Guid? SourceId,
        string? SourceSlug,
        string Title,
        string Text)
    {
        public static ContentDraft ForProject(Project project, string text) =>
            new("project", project.Id, project.Slug, project.Title, text);

        public static ContentDraft ForBlogPost(BlogPost post, string text) =>
            new("blog_post", post.Id, post.Slug, post.Title, text);

        public static ContentDraft ForExperience(Experience experience, string text) =>
            new("experience", experience.Id, null, $"{experience.Title} at {experience.Company}", text);

        public static ContentDraft ForCategory(string category, string text) =>
            new("skill", null, null, $"Skills — {category}", text);

        public static ContentDraft ForProfile(string title, string text) =>
            new("profile", null, null, title, text);
    }
}