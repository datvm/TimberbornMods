namespace ModdableTimberborn.BuildingSettings.BuiltInSettings;

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
)
{

    [JsonIgnore]
    public SluiceState SluiceState { get; } = new(null)
    {
        AutoMode = AutoMode,
        IsOpen = IsOpen,
        AutoCloseOnOutflow = AutoCloseOnOutflow,
        OutflowLimit = OutflowLimit,
        AutoCloseOnAbove = AutoCloseOnAbove,
        OnAboveLimit = OnAboveLimit,
        OnBelowLimit = OnBelowLimit,
        IsSynchronized = IsSynchronized
    };
}

public class SluiceSettings(ILoc t) : BuildingSettingsBase<Sluice, SluiceSettingsModel>(t)
{
    public override string DescribeModel(SluiceSettingsModel model) => "";

    protected override bool ApplyModel(SluiceSettingsModel model, Sluice target)
    {
        target._sluiceState.SetStateAndSynchronize(model.SluiceState, target.MinHeight - target.MaxHeight);

        return true;
    }

    protected override SluiceSettingsModel GetModel(Sluice duplicable)
    {
        var state = duplicable._sluiceState;

        return new(
            state.AutoMode,
            state.IsOpen,
            state.AutoCloseOnOutflow,
            state.OnAboveLimit,
            state.AutoCloseOnAbove,
            state.AutoCloseOnBelow,
            state.OnAboveLimit,
            state.OnBelowLimit,
            state.IsSynchronized
        );
    }
}