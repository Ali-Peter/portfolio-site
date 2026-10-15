using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Pgvector;
using Portfolio.Domain.Entities;

namespace Portfolio.Infrastructure.Persistence.Configurations;

public class ContentChunkConfiguration : IEntityTypeConfiguration<ContentChunk>
{
    public void Configure(EntityTypeBuilder<ContentChunk> builder)
    {
        builder.ToTable("content_chunks");

        builder.Property(c => c.SourceType).IsRequired().HasMaxLength(30);
        builder.Property(c => c.SourceSlug).HasMaxLength(200);
        builder.Property(c => c.Title).IsRequired().HasMaxLength(300);
        builder.Property(c => c.Content).IsRequired();

        // pgvector column; dimensions must match text-embedding-3-small.
        builder.Property(c => c.Embedding)
            .HasColumnType("vector(1536)");

        builder.HasIndex(c => c.SourceType);
        builder.HasIndex(c => c.SourceId);

        // Chunks are a derived index; soft-deleted rows are invisible to search.
        builder.HasQueryFilter(c => !c.IsDeleted);
    }
}