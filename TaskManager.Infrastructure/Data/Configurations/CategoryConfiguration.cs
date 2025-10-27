using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TaskManager.Core.Entities;

namespace TaskManager.Infrastructure.Data.Configurations;

public class CategoryConfiguration : IEntityTypeConfiguration<Category>
{
    public void Configure(EntityTypeBuilder<Category> builder)
    {
        builder.HasKey(c => c.Id);

        builder.Property(c => c.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(c => c.Description)
            .HasMaxLength(500);

        builder.Property(c => c.Color)
            .IsRequired()
            .HasMaxLength(7);

        builder.Property(c => c.CreatedAt)
            .IsRequired();

        builder.HasMany(c => c.Tasks)
            .WithOne(t => t.Category)
            .HasForeignKey(t => t.CategoryId);

        builder.HasIndex(c => c.Name);
    }
}
