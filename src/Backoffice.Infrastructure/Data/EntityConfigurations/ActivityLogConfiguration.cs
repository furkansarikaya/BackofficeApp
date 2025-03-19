using Backoffice.Domain.Entities.Auditing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Backoffice.Infrastructure.Data.EntityConfigurations;

public class ActivityLogConfiguration : IEntityTypeConfiguration<ActivityLog>
{
    public void Configure(EntityTypeBuilder<ActivityLog> builder)
    {
        builder.HasKey(e => e.Id);
        
        builder.Property(e => e.UserId)
            .IsRequired()
            .HasMaxLength(50);
        
        builder.Property(e => e.UserName)
            .IsRequired()
            .HasMaxLength(100);
        
        builder.Property(e => e.Category)
            .IsRequired()
            .HasMaxLength(50);
        
        builder.Property(e => e.ActivityType)
            .IsRequired()
            .HasMaxLength(50);
        
        builder.Property(e => e.EntityType)
            .HasMaxLength(100);
        
        builder.Property(e => e.EntityId)
            .HasMaxLength(100);
        
        builder.Property(e => e.Details)
            .HasColumnType("text");
        
        builder.Property(e => e.IpAddress)
            .IsRequired()
            .HasMaxLength(50);
        
        builder.Property(e => e.UserAgent)
            .HasMaxLength(500);
        
        builder.Property(e => e.Timestamp)
            .IsRequired();
        
        // Indeksler oluştur
        builder.HasIndex(e => e.Timestamp);
        builder.HasIndex(e => e.UserId);
        builder.HasIndex(e => e.Category);
        builder.HasIndex(e => e.ActivityType);
    }
}