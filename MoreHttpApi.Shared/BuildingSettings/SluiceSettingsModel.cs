namespace MoreHttpApi.Shared.BuildingSettings;

public record SluiceSettingsModel(
    bool AutoMode,
    bool IsOpen,
    bool AutoCloseOnOutflow,
    float OutflowLimit,
    bool AutoCloseOnAbove,
    bool AutoCloseOnBelow,
    float OnAboveLimit,
    float OnBelowLimit,
    bool IsSynchronized
);