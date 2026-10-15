using Microsoft.EntityFrameworkCore;
using Portfolio.Domain.Entities;

namespace Portfolio.Infrastructure.Persistence;

/// <summary>
/// Idempotent seed using the real content from the original portfolio.
/// Split into partial files by content type; skipped entirely when data already exists.
/// </summary>
public static partial class SeedData
{
    private const string SeedUser = "seed";

    public static async Task RunAsync(PortfolioDbContext db)
    {
        if (await db.Projects.AsNoTracking().AnyAsync())
        {
            return; // database already contains data — nothing to do
        }

        db.Projects.AddRange(CreateProjects());
        db.Skills.AddRange(CreateSkills());
        db.Experiences.AddRange(CreateExperiences());
        db.BlogPosts.AddRange(CreateBlogPosts());

        foreach (var entry in db.ChangeTracker.Entries<BaseEntity>())
        {
            entry.Entity.CreatedBy = SeedUser;
        }

        await db.SaveChangesAsync();
    }
}
