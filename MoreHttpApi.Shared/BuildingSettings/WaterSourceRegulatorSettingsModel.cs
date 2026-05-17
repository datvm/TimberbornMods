namespace MoreHttpApi.Shared.BuildingSettings;

public record WaterSourceRegulatorSettingsModel(HttpRegulatorState State);

public enum HttpRegulatorState
{
    Open,
    Closed,
    Automated
}
