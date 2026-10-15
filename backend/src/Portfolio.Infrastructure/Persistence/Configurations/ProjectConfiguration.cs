using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Portfolio.Domain.Entities;
using Portfolio.Domain.Enums;

namespace Portfolio.Infrastructure.Persistence.Configurations;

public class ProjectConfiguration : IEntityTypeConfiguration<Project>
{
    public void Configure(EntityTypeBuilder<Project> builder)
    {
        builder.ToTable("projects");

        builder.Property(p => p.Slug).IsRequired().HasMaxLength(200);
        builder.Property(p => p.Title).IsRequired().HasMaxLength(200);
        builder.Property(p => p.Summary).IsRequired().HasMaxLength(500);
        builder.Property(p => p.Description).IsRequired();
        builder.Property(p => p.RepoUrl).HasMaxLength(1000);
        builder.Property(p => p.DemoUrl).HasMaxLength(1000);
        builder.Property(p => p.CoverImageUrl).HasMaxLength(1000);
        builder.Property(p => p.Status)
            .HasConversion<string>()
            .HasMaxLength(20);
        builder.Property(p => p.Technologies)
            .HasColumnType("text[]")
            .IsRequired();

        builder.HasIndex(p => p.Slug).IsUnique();
        builder.HasIndex(p => p.Featured);
        builder.HasIndex(p => p.SortOrder);

        builder.HasQueryFilter(p => !p.IsDeleted);
    }
}
