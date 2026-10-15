using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Portfolio.Domain.Entities;
using Portfolio.Domain.Enums;

namespace Portfolio.Infrastructure.Persistence.Configurations;

public class SkillConfiguration : IEntityTypeConfiguration<Skill>
{
    public void Configure(EntityTypeBuilder<Skill> builder)
    {
        builder.ToTable("skills");

        builder.Property(s => s.Name).IsRequired().HasMaxLength(100);
        builder.Property(s => s.Category)
            .HasConversion<string>()
            .HasMaxLength(20);
        builder.Property(s => s.Proficiency)
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.HasIndex(s => s.Name).IsUnique();
        builder.HasIndex(s => s.Category);

        builder.HasQueryFilter(s => !s.IsDeleted);
    }
}
