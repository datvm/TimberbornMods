namespace ModdableTimberborn.BuildingSettings.BuiltInSettings;

public record IndicatorSettingsModel(
    IndicatorPinnedMode PinnedMode,
    bool IsWarningEnabled,
    bool IsJournalEntryEnabled,
    bool IsColorReplicationEnabled
);

public class IndicatorSettings(ILoc t) : BuildingSettingsBase<Indicator, IndicatorSettingsModel>(t)
{
    public override string DescribeModel(IndicatorSettingsModel model) => "";

    protected override bool ApplyModel(IndicatorSettingsModel model, Indicator target)
    {
        target.SetPinnedMode(model.PinnedMode);
        target.SetWarningEnabled(model.IsWarningEnabled);
        target.SetJournalEntryEnabled(model.IsJournalEntryEnabled);
        target.SetColorReplicationEnabled(model.IsColorReplicationEnabled);

        return true;
    }

    protected override IndicatorSettingsModel GetModel(Indicator target)
        => new(target.PinnedMode, target.IsWarningEnabled, target.IsJournalEntryEnabled, target.IsColorReplicationEnabled);
}