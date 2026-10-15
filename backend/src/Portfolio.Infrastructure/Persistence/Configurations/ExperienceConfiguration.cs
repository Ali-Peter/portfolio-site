using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Portfolio.Domain.Entities;

namespace Portfolio.Infrastructure.Persistence.Configurations;

public class ExperienceConfiguration : IEntityTypeConfiguration<Experience>
{
    public void Configure(EntityTypeBuilder<Experience> builder)
    {
        builder.ToTable("experiences");

        builder.Property(e => e.Company).IsRequired().HasMaxLength(200);
        builder.Property(e => e.Title).IsRequired().HasMaxLength(200);
        builder.Property(e => e.Description).IsRequired();
        builder.Property(e => e.Technologies)
            .HasColumnType("text[]")
            .IsRequired();

        builder.HasIndex(e => e.SortOrder);

        builder.HasQueryFilter(e => !e.IsDeleted);
    }
}
