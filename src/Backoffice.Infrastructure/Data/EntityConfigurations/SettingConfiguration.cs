using Backoffice.Domain.Entities.Settings;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Backoffice.Infrastructure.Data.EntityConfigurations;

public class SettingConfiguration : IEntityTypeConfiguration<Setting>
{
    public void Configure(EntityTypeBuilder<Setting> builder)
    {
        builder.HasKey(s => s.Id);
        
        builder.Property(s => s.Key)
            .IsRequired()
            .HasMaxLength(100);
        
        builder.Property(s => s.Value)
            .IsRequired()
            .HasColumnType("text");
        
        builder.Property(s => s.Description)
            .HasMaxLength(500);
        
        builder.Property(s => s.DataType)
            .IsRequired()
            .HasMaxLength(50)
            .HasDefaultValue("string");
        
        builder.Property(s => s.IsEncrypted)
            .IsRequired()
            .HasDefaultValue(false);
            
        builder.Property(s => s.IsReadOnly)
            .IsRequired()
            .HasDefaultValue(false);
            
        // Create unique index on Key
        builder.HasIndex(s => s.Key)
            .IsUnique();
    }
}