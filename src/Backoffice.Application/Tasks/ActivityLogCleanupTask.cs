using Backoffice.Application.Common.Interfaces;

namespace Backoffice.Application.Tasks;

public class ActivityLogCleanupTask(IUnitOfWork unitOfWork, IDbLoggerService logger) : IScheduledTask
{
    public string Name => "ActivityLog Temizleme";
    public string Description => "Belirli bir süreden eski activity log kayıtlarını temizler";

    public async Task<string> ExecuteAsync(Dictionary<string, string> parameters, CancellationToken cancellationToken = default)
    {
        if (!parameters.TryGetValue("retentionDays", out var retentionDaysStr) || 
            !int.TryParse(retentionDaysStr, out var retentionDays))
        {
            retentionDays = 90; // Default 90 gün (3 ay)
        }

        var cutoffDate = DateTime.UtcNow.AddDays(-retentionDays);
        await logger.LogInformationAsync($"ActivityLog temizleme işlemi başladı. Kesim tarihi: {cutoffDate}", "ActivityLogCleanupTask");

        var repository = unitOfWork.Repository<Domain.Entities.Auditing.ActivityLog, long>();
        
        // Veritabanı seviyesinde silme işlemi (daha hızlı)
        var deleteQuery = $"DELETE FROM activity_logs WHERE timestamp < '{cutoffDate:yyyy-MM-dd HH:mm:ss}'";
        var rowsDeleted = await repository.ExecuteNativeQueryAsync(deleteQuery);

        var result = $"{rowsDeleted} adet ActivityLog kaydı silindi. Kesim tarihi: {cutoffDate}";
        await logger.LogInformationAsync(result, "ActivityLogCleanupTask");
        
        return result;
    }

    public Dictionary<string, string> GetDefaultParameters()
    {
        return new Dictionary<string, string>
        {
            { "retentionDays", "90" } // 3 ay
        };
    }
}