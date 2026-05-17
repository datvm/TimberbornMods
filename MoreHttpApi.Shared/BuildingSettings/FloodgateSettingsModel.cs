namespace MoreHttpApi.Shared.BuildingSettings;

public record FloodgateSettingsModel(
    float Height,
    bool IsSynchronized,
    float AutomationHeight = 0
);