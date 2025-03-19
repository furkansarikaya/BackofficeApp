using Backoffice.Domain.Entities.Menu;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Backoffice.Infrastructure.Data.EntityConfigurations;

public class MenuItemConfiguration : IEntityTypeConfiguration<MenuItem>
{
    public void Configure(EntityTypeBuilder<MenuItem> builder)
    {
        builder.HasKey(m => m.Id);

        builder.Property(m => m.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(m => m.Icon)
            .HasMaxLength(50);

        builder.Property(m => m.Controller)
            .HasMaxLength(100);

        builder.Property(m => m.Action)
            .HasMaxLength(100);

        builder.Property(m => m.Url)
            .HasMaxLength(500);

        builder.Property(m => m.RequiredPermissionCode)
            .HasMaxLength(100);

        // Self-referencing relationship for hierarchical menu
        builder.HasOne(m => m.Parent)
            .WithMany(m => m.Children)
            .HasForeignKey(m => m.ParentId)
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired(false);
    }
}