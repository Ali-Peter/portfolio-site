using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Portfolio.Domain.Entities;

namespace Portfolio.Infrastructure.Persistence.Configurations;

public class BlogPostConfiguration : IEntityTypeConfiguration<BlogPost>
{
    public void Configure(EntityTypeBuilder<BlogPost> builder)
    {
        builder.ToTable("blog_posts");

        builder.Property(b => b.Slug).IsRequired().HasMaxLength(200);
        builder.Property(b => b.Title).IsRequired().HasMaxLength(300);
        builder.Property(b => b.Excerpt).IsRequired().HasMaxLength(1000);
        builder.Property(b => b.BodyMarkdown).IsRequired();
        builder.Property(b => b.CoverImageUrl).HasMaxLength(1000);
        builder.Property(b => b.Tags)
            .HasColumnType("text[]")
            .IsRequired();

        builder.HasIndex(b => b.Slug).IsUnique();
        builder.HasIndex(b => b.IsPublished);
        builder.HasIndex(b => b.PublishedAt);

        builder.HasQueryFilter(b => !b.IsDeleted);
    }
}
