using Backoffice.Domain.Entities.Security;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Backoffice.Infrastructure.Data.EntityConfigurations;

public class IpFilterConfiguration : IEntityTypeConfiguration<IpFilter>
{
    public void Configure(EntityTypeBuilder<IpFilter> builder)
    {
        builder.HasKey(p => p.Id);
        
        builder.Property(p => p.IpAddress)
            .IsRequired()
            .HasMaxLength(50);
        
        builder.Property(p => p.Description)
            .HasMaxLength(255);
        
        builder.Property(p => p.FilterType)
            .IsRequired();
        
        builder.Property(p => p.IsActive)
            .IsRequired()
            .HasDefaultValue(true);
    }
}