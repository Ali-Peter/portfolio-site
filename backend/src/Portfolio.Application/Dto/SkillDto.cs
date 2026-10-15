using Portfolio.Domain.Enums;

namespace Portfolio.Application.Dto;

/// <summary>Skill badge payload returned by the public API.</summary>
public sealed record SkillDto(
    string Name,
    SkillCategory Category,
    SkillProficiency Proficiency,
    int SortOrder);
