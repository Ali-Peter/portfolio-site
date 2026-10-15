using Portfolio.Domain.Enums;

namespace Portfolio.Domain.Entities;

/// <summary>
/// A technology/skill badge rendered in the expertise section.
/// </summary>
public class Skill : BaseEntity
{
    public string Name { get; set; } = string.Empty;

    public SkillCategory Category { get; set; }

    public SkillProficiency Proficiency { get; set; } = SkillProficiency.Proficient;

    /// <summary>Lower numbers sort first within a category.</summary>
    public int SortOrder { get; set; }
}
