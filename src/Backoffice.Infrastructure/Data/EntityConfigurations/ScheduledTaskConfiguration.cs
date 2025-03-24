using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Backoffice.Infrastructure.Data.EntityConfigurations;

public class ScheduledTaskConfiguration : IEntityTypeConfiguration<Domain.Entities.ScheduledTasks.ScheduledTask>
{
    public void Configure(EntityTypeBuilder<Domain.Entities.ScheduledTasks.ScheduledTask> builder)
    {
        builder.HasKey(t => t.Id);

        builder.Property(t => t.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(t => t.TaskType)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(t => t.Description)
            .HasMaxLength(500);

        builder.Property(t => t.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(t => t.Interval)
            .IsRequired()
            .HasConversion(
                v => v.Ticks,
                v => TimeSpan.FromTicks(v));

        builder.Property(t => t.LastRunTime)
            .IsRequired(false);

        builder.Property(t => t.NextRunTime)
            .IsRequired(false);

        builder.Property(t => t.IsRunning)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(t => t.LastRunResult)
            .HasMaxLength(2000);

        builder.Property(t => t.ParametersJson)
            .HasColumnName("Parameters")
            .HasColumnType("jsonb")
            .HasDefaultValueSql("'{}'::jsonb");

        builder.Ignore(t => t.Parameters);
    }
}