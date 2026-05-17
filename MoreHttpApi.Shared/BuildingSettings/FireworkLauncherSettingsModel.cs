namespace MoreHttpApi.Shared.BuildingSettings;

public record FireworkLauncherSettingsModel(
    string? FireworkId,
    int Heading,
    int Pitch,
    int Distance,
    bool IsContinuous
);
