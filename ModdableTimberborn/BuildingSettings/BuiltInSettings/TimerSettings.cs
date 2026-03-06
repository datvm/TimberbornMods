using Timer = Timberborn.AutomationBuildings.Timer;

namespace ModdableTimberborn.BuildingSettings.BuiltInSettings;

public record TimerSettingsIntervalModel(
    IntervalType Type,
    int Ticks,
    float? Hours
);

public record TimerSettingsModel(
    TimerSettingsIntervalModel IntervalA,
    TimerSettingsIntervalModel IntervalB,
    Guid? Input,
    Guid? ResetInput,
    TimerMode Mode
) : EntityIdModelBase([Input, ResetInput])
{
    public Guid? Input
    {
        get => EntityIds[0];
        set => EntityIds[0] = value;
    }

    public Guid? ResetInput
    {
        get => EntityIds[1];
        set => EntityIds[1] = value;
    }
}

public class TimerSettings(
    EntityRegistry entityRegistry,
    ILoc t
) : EntityIdBuildingSettingsBase<Timer, TimerSettingsModel>(t)
{
    public override string DescribeModel(TimerSettingsModel model) => "";

    protected override bool ApplyModel(TimerSettingsModel model, Timer target)
    {
        ApplyInterval(target.TimerIntervalA, model.IntervalA);
        ApplyInterval(target.TimerIntervalB, model.IntervalB);

        target.SetInput(entityRegistry.TryGetAutomator(model.Input));
        target.SetResetInput(entityRegistry.TryGetAutomator(model.ResetInput));
        target.SetMode(model.Mode);

        return true;
    }

    protected override TimerSettingsModel GetModel(Timer target)
        => new(
            CreateFrom(target.TimerIntervalA),
            CreateFrom(target.TimerIntervalB),
            target.Input?.GetEntityId(),
            target.ResetInput?.GetEntityId(),
            target.Mode
        );

    static TimerSettingsIntervalModel CreateFrom(TimerInterval i) => new(i.Type, i.Ticks, i._hours);

    static void ApplyInterval(TimerInterval i, TimerSettingsIntervalModel m)
    {
        i.Type = m.Type;
        i.Ticks = m.Ticks;
        i._hours = m.Hours ?? 0;
    }
}