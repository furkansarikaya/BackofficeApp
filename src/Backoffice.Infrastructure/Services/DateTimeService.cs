using Backoffice.Application.Common.Interfaces;

namespace Backoffice.Infrastructure.Services;

public class DateTimeService : IDateTimeService
{
    public DateTime Now => DateTime.UtcNow;
    public DateTime ConvertCurrentTimeZoneToUtc(DateTime dateTime)
    {
        var localTimeZone = TimeZoneInfo.Local;
        var utcDateTime = TimeZoneInfo.ConvertTimeToUtc(dateTime, localTimeZone);
        return utcDateTime;
    }

    public DateTime ConvertUtcToCurrentTimeZone(DateTime dateTime)
    {
        var localTimeZone = TimeZoneInfo.Local;
        var localDateTime = TimeZoneInfo.ConvertTimeFromUtc(dateTime, localTimeZone);
        return localDateTime;
    }
}