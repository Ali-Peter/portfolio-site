using Portfolio.Application.Dto;

namespace Portfolio.Application.Services;

/// <summary>Public resume/experience query.</summary>
public interface IExperienceService
{
    Task<IReadOnlyList<ExperienceDto>> GetExperiencesAsync(
        CancellationToken cancellationToken = default);
}
