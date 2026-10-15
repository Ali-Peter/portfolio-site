using Microsoft.EntityFrameworkCore;
using Portfolio.Application.Persistence;
using Portfolio.Domain.Entities;

namespace Portfolio.Infrastructure.Persistence;

/// <summary>
/// EF Core context for the portfolio database.
/// Owns entity configuration, the global soft-delete filter and audit stamping.
/// Implements <see cref="IPortfolioDbContext"/> so Application services
/// never depend on this concrete type.
/// </summary>
public class PortfolioDbContext : DbContext, IPortfolioDbContext
{
    public PortfolioDbContext(DbContextOptions<PortfolioDbContext> options)
        : base(options)
    {
    }

    public DbSet<Project> Projects => Set<Project>();

    public DbSet<BlogPost> BlogPosts => Set<BlogPost>();

    public DbSet<Skill> Skills => Set<Skill>();

    public DbSet<Experience> Experiences => Set<Experience>();

    public DbSet<ContactMessage> ContactMessages => Set<ContactMessage>();

    public DbSet<ContentChunk> ContentChunks => Set<ContentChunk>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(PortfolioDbContext).Assembly);
    }

    public override int SaveChanges()
    {
        ApplyAuditRules();
        return base.SaveChanges();
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        ApplyAuditRules();
        return base.SaveChangesAsync(cancellationToken);
    }

    private void ApplyAuditRules()
    {
        var now = DateTime.UtcNow;

        foreach (var entry in ChangeTracker.Entries<BaseEntity>())
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    entry.Entity.CreatedAtUtc = now;
                    break;
                case EntityState.Modified:
                    entry.Entity.UpdatedAtUtc = now;
                    break;
            }
        }
    }
}
