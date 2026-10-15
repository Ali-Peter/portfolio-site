using Microsoft.EntityFrameworkCore;
using Portfolio.Domain.Entities;

namespace Portfolio.Application.Persistence;

/// <summary>
/// Persistence abstraction owned by the Application layer so services never
/// depend on Infrastructure (dependency rule from ADR-0001).
/// Implemented by <c>PortfolioDbContext</c> in Portfolio.Infrastructure.
/// </summary>
public interface IPortfolioDbContext
{
    DbSet<Project> Projects { get; }

    DbSet<BlogPost> BlogPosts { get; }

    DbSet<Skill> Skills { get; }

    DbSet<Experience> Experiences { get; }

    DbSet<ContactMessage> ContactMessages { get; }

    DbSet<ContentChunk> ContentChunks { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
