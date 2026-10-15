using Portfolio.Application.Dto;

namespace Portfolio.Application.Services;

/// <summary>
/// Admin content management: CRUD over portfolio entities plus the contact inbox.
/// Reads include drafts/unpublished rows the public API hides; writes use
/// soft-delete so history is never lost. Controllers trigger the RAG reindex
/// after mutating calls — the service stays persistence-only.
/// </summary>
public interface IAdminContentService
{
    // -- Projects --
    Task<IReadOnlyList<AdminProjectDto>> GetProjectsAsync(CancellationToken ct = default);
    Task<AdminProjectDto> GetProjectAsync(Guid id, CancellationToken ct = default);
    Task<AdminProjectDto> CreateProjectAsync(UpsertProjectRequest request, string actor, CancellationToken ct = default);
    Task<AdminProjectDto> UpdateProjectAsync(Guid id, UpsertProjectRequest request, string actor, CancellationToken ct = default);
    Task DeleteProjectAsync(Guid id, CancellationToken ct = default);

    // -- Blog --
    Task<IReadOnlyList<AdminBlogPostDto>> GetBlogPostsAsync(CancellationToken ct = default);
    Task<AdminBlogPostDto> GetBlogPostAsync(Guid id, CancellationToken ct = default);
    Task<AdminBlogPostDto> CreateBlogPostAsync(UpsertBlogPostRequest request, string actor, CancellationToken ct = default);
    Task<AdminBlogPostDto> UpdateBlogPostAsync(Guid id, UpsertBlogPostRequest request, string actor, CancellationToken ct = default);
    Task DeleteBlogPostAsync(Guid id, CancellationToken ct = default);

    // -- Skills --
    Task<IReadOnlyList<AdminSkillDto>> GetSkillsAsync(CancellationToken ct = default);
    Task<AdminSkillDto> GetSkillAsync(Guid id, CancellationToken ct = default);
    Task<AdminSkillDto> CreateSkillAsync(UpsertSkillRequest request, string actor, CancellationToken ct = default);
    Task<AdminSkillDto> UpdateSkillAsync(Guid id, UpsertSkillRequest request, string actor, CancellationToken ct = default);
    Task DeleteSkillAsync(Guid id, CancellationToken ct = default);

    // -- Experience --
    Task<IReadOnlyList<AdminExperienceDto>> GetExperiencesAsync(CancellationToken ct = default);
    Task<AdminExperienceDto> GetExperienceAsync(Guid id, CancellationToken ct = default);
    Task<AdminExperienceDto> CreateExperienceAsync(UpsertExperienceRequest request, string actor, CancellationToken ct = default);
    Task<AdminExperienceDto> UpdateExperienceAsync(Guid id, UpsertExperienceRequest request, string actor, CancellationToken ct = default);
    Task DeleteExperienceAsync(Guid id, CancellationToken ct = default);

    // -- Contact inbox --
    Task<PagedResult<AdminContactMessageDto>> GetContactMessagesAsync(
        int page, int pageSize, bool? handled, CancellationToken ct = default);
    Task MarkContactHandledAsync(Guid id, bool handled, CancellationToken ct = default);
    Task DeleteContactMessageAsync(Guid id, CancellationToken ct = default);
}
