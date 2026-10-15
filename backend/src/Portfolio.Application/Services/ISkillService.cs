using Portfolio.Application.Dto;

namespace Portfolio.Application.Services;

/// <summary>Public skill badge query.</summary>
public interface ISkillService
{
    Task<IReadOnlyList<SkillDto>> GetSkillsAsync(
        CancellationToken cancellationToken = default);
}
