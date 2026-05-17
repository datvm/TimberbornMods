namespace MoreHttpApi.Shared.BuildingSettings;

public record ValveSettingsModel(
    bool IsSynchronized,
    float OutflowLimit,
    bool OutflowLimitEnabled,
    float AutomationOutflowLimit,
    bool AutomationOutflowLimitEnabled,
    float ReactionSpeed
);
