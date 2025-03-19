namespace Backoffice.Application.Common.Interfaces;

public interface IDateTimeService
{
    DateTime Now { get; }
    DateTime ConvertCurrentTimeZoneToUtc(DateTime dateTime);
    DateTime ConvertUtcToCurrentTimeZone(DateTime dateTime);
}