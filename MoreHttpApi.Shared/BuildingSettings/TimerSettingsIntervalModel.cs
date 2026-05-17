namespace MoreHttpApi.Shared.BuildingSettings;

public record TimerSettingsIntervalModel(
    HttpIntervalType Type,
    int Ticks,
    float? Hours
);

public enum HttpIntervalType
{
    Ticks,
    Hours,
    Days
}
