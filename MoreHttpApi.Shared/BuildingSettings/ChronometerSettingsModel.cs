namespace MoreHttpApi.Shared.BuildingSettings;

public record ChronometerSettingsModel(float StartTime, float EndTime, HttpChronometerMode Mode);

public enum HttpChronometerMode
{
    TimeRange,
    WorkingHours,
    NonWorkingHours
}
