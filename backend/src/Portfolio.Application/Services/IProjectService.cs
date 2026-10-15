using Portfolio.Application.Dto;

namespace Portfolio.Application.Services;

/// <summary>Public project catalogue queries.</summary>
public interface IProjectService
{
    Task<PagedResult<ProjectDto>> GetProjectsAsync(
        int page,
        int pageSize,
        string? tag,
        CancellationToken cancellationToken = default);

    Task<ProjectDto> GetProjectBySlugAsync(
        string slug,
        CancellationToken cancellationToken = default);
}
