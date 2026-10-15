using Pgvector;

namespace Portfolio.Domain.Entities;

/// <summary>
/// A retrieval-augmented generation (RAG) chunk: a slice of portfolio content
/// (project, blog post, skill category, experience entry, or profile overview)
/// paired with its OpenAI embedding, stored in the pgvector-backed
/// <c>content_chunks</c> table. Chunks are a derived index — they carry no data
/// of their own and can be rebuilt from the source entities at any time.
/// </summary>
public class ContentChunk : BaseEntity
{
    /// <summary>Source kind: "project" | "blog_post" | "skill" | "experience" | "profile".</summary>
    public string SourceType { get; set; } = string.Empty;

    /// <summary>Id of the source entity when one exists (null for the profile overview).</summary>
    public Guid? SourceId { get; set; }

    /// <summary>URL slug of the source entity when one exists (null for skills/profile).</summary>
    public string? SourceSlug { get; set; }

    /// <summary>Human-readable source title, e.g. the project or article title.</summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>The chunk text that gets embedded and passed to the model as context.</summary>
    public string Content { get; set; } = string.Empty;

    /// <summary>Embedding vector (text-embedding-3-small, 1536 dimensions).</summary>
    public Vector? Embedding { get; set; }
}