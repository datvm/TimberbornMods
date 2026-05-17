namespace MoreHttpApi.Shared.BuildingSettings;

public record PowerMeterSettingsModel(
    HttpPowerMeterMode Mode,
    HttpNumericComparisonMode ComparisonMode,
    int IntThreshold,
    float PercentThreshold
);

public enum HttpPowerMeterMode
{
    Supply,
    Demand,
    Surplus,
    BatteryChargeLevel
}
