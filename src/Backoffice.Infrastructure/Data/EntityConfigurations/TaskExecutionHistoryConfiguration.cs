using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Backoffice.Infrastructure.Data.EntityConfigurations;

public class TaskExecutionHistoryConfiguration : IEntityTypeConfiguration<Domain.Entities.ScheduledTasks.TaskExecutionHistory>
{
    public void Configure(EntityTypeBuilder<Domain.Entities.ScheduledTasks.TaskExecutionHistory> builder)
    {
        builder.HasKey(t => t.Id);
        
        builder.Property(t => t.StartTime)
            .IsRequired();
            
        builder.Property(t => t.EndTime)
            .IsRequired(false);
            
        builder.Property(t => t.IsSuccess)
            .IsRequired();
            
        builder.Property(t => t.Result)
            .HasMaxLength(2000);
            
        builder.Property(t => t.ErrorMessage)
            .HasColumnType("text");
            
        builder.HasOne(t => t.Task)
            .WithMany()
            .HasForeignKey(t => t.TaskId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}