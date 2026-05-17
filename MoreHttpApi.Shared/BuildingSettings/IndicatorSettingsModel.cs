namespace MoreHttpApi.Shared.BuildingSettings;

public record IndicatorSettingsModel(
    HttpIndicatorPinnedMode PinnedMode,
    bool IsWarningEnabled,
    bool IsJournalEntryEnabled,
    bool IsColorReplicationEnabled
);

public enum HttpIndicatorPinnedMode
{
    Never,
    WhenOn,
    Always
}
