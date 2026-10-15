using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Portfolio.Domain.Entities;

namespace Portfolio.Infrastructure.Persistence.Configurations;

public class ContactMessageConfiguration : IEntityTypeConfiguration<ContactMessage>
{
    public void Configure(EntityTypeBuilder<ContactMessage> builder)
    {
        builder.ToTable("contact_messages");

        builder.Property(c => c.Name).IsRequired().HasMaxLength(200);
        builder.Property(c => c.Email).IsRequired().HasMaxLength(320);
        builder.Property(c => c.Message).IsRequired();
        builder.Property(c => c.IpHash).HasMaxLength(100);

        builder.HasIndex(c => c.IsHandled);
        builder.HasIndex(c => c.CreatedAtUtc);

        builder.HasQueryFilter(c => !c.IsDeleted);
    }
}
