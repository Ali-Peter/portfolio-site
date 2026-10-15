using Portfolio.Domain.Entities;
using Portfolio.Domain.Enums;

namespace Portfolio.Infrastructure.Persistence;

public static partial class SeedData
{
    // Skill content is real, professional-level content: the daily stack
    // (C#, ASP.NET Core, Angular, SQL Server, PostgreSQL) plus the AI stack
    // used on the Outbound AI product. WinForms/Crystal Reports and
    // beginner-branded entries were dropped from the skills list; legacy
    // WinForms work still appears (truthfully) under the GlobalSuite ERP
    // project's technology tags.
    private static Skill[] CreateSkills() =>
    [
        // Backend
        Skill("C#", SkillCategory.Backend, 1, SkillProficiency.Advanced),
        Skill("ASP.NET Core", SkillCategory.Backend, 2, SkillProficiency.Advanced),
        Skill(".NET Framework", SkillCategory.Backend, 3, SkillProficiency.Proficient),
        Skill("FastAPI", SkillCategory.Backend, 4, SkillProficiency.Advanced),
        Skill("Node.js", SkillCategory.Backend, 5, SkillProficiency.Proficient),
        Skill("Express.js", SkillCategory.Backend, 6, SkillProficiency.Proficient),
        Skill("Python", SkillCategory.Backend, 7, SkillProficiency.Proficient),

        // Frontend
        Skill("Angular", SkillCategory.Frontend, 8, SkillProficiency.Advanced),
        Skill("React", SkillCategory.Frontend, 9, SkillProficiency.Proficient),
        Skill("TypeScript", SkillCategory.Frontend, 10, SkillProficiency.Proficient),
        Skill("JavaScript", SkillCategory.Frontend, 11, SkillProficiency.Proficient),
        Skill("Tailwind CSS", SkillCategory.Frontend, 12, SkillProficiency.Proficient),
        Skill("HTML", SkillCategory.Frontend, 13, SkillProficiency.Proficient),
        Skill("CSS", SkillCategory.Frontend, 14, SkillProficiency.Proficient),

        // Data & infrastructure
        Skill("PostgreSQL", SkillCategory.DataAndInfra, 15, SkillProficiency.Advanced),
        Skill("SQL Server", SkillCategory.DataAndInfra, 16, SkillProficiency.Advanced),
        Skill("Stored Procedures", SkillCategory.DataAndInfra, 17, SkillProficiency.Advanced),
        Skill("MongoDB", SkillCategory.DataAndInfra, 18, SkillProficiency.Proficient),
        Skill("Docker", SkillCategory.DataAndInfra, 19, SkillProficiency.Proficient),
        Skill("Azure", SkillCategory.DataAndInfra, 20, SkillProficiency.Proficient),
        Skill("GitHub Actions", SkillCategory.DataAndInfra, 21, SkillProficiency.Proficient),
        Skill("Celery", SkillCategory.DataAndInfra, 22, SkillProficiency.Proficient),

        // AI / ML (real production experience via the Outbound AI product)
        Skill("Deepgram", SkillCategory.AiMl, 23, SkillProficiency.Advanced),
        Skill("ElevenLabs", SkillCategory.AiMl, 24, SkillProficiency.Advanced),

        // Tools
        Skill("Git", SkillCategory.Tools, 25, SkillProficiency.Advanced),
        Skill("Swagger / OpenAPI", SkillCategory.Tools, 26, SkillProficiency.Proficient),
        Skill("REST API Design", SkillCategory.Tools, 27, SkillProficiency.Advanced),
    ];

    private static Skill Skill(
        string name,
        SkillCategory category,
        int sortOrder,
        SkillProficiency? proficiency = null) =>
        new()
        {
            Name = name,
            Category = category,
            SortOrder = sortOrder,
            Proficiency = proficiency ?? SkillProficiency.Proficient,
        };
}
