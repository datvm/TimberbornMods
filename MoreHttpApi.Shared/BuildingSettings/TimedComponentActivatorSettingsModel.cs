namespace MoreHttpApi.Shared.BuildingSettings;

public readonly record struct TimedComponentActivatorSettingsModel(
    bool Enabled,
    int CyclesUntilCountdownActivation,
    float DaysUntilActivation);