namespace MoreHttpApi.Shared.BuildingSettings;

public record WaterInputCoordinatesSettingsModel(
    bool UseDepthLimit,
    int DepthLimit
);
