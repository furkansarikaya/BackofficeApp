using Backoffice.Domain.Entities.Logging;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Backoffice.Infrastructure.Data.EntityConfigurations;

public class LogEntryConfiguration : IEntityTypeConfiguration<LogEntry>
{
    public void Configure(EntityTypeBuilder<LogEntry> builder)
    {
        builder.ToTable("log_entries");
        
        builder.HasKey(e => e.Id);
        
        builder.Property(e => e.Level)
            .IsRequired()
            .HasConversion<byte>();
        
        builder.Property(e => e.Message)
            .IsRequired()
            .HasColumnType("text");
        
        builder.Property(e => e.Category)
            .IsRequired()
            .HasMaxLength(100);
        
        builder.Property(e => e.Exception)
            .HasColumnType("text");
        
        builder.Property(e => e.StackTrace)
            .HasColumnType("text");
        
        builder.Property(e => e.UserId)
            .HasMaxLength(100);
        
        builder.Property(e => e.UserName)
            .HasMaxLength(100);
        
        builder.Property(e => e.RequestPath)
            .HasMaxLength(500);
        
        builder.Property(e => e.RequestMethod)
            .HasMaxLength(10);
        
        builder.Property(e => e.IpAddress)
            .HasMaxLength(50);
        
        builder.Property(e => e.UserAgent)
            .HasMaxLength(500);
        
        builder.Property(e => e.AdditionalData)
            .HasColumnType("jsonb");
        
        builder.Property(e => e.Timestamp)
            .IsRequired();
        
        // Indeksler
        builder.HasIndex(e => e.Timestamp);
        builder.HasIndex(e => e.Level);
        builder.HasIndex(e => e.Category);
        builder.HasIndex(e => e.UserId);
    }
}