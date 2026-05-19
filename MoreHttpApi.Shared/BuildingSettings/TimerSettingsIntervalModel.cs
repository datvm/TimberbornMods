namespace MoreHttpApi.Shared.BuildingSettings;

public record TimerSettingsIntervalModel(
    HttpIntervalType Type,
    int Ticks,
    float? Hours
);

public record TimerSettingsModel(
    TimerSettingsIntervalModel IntervalA,
    TimerSettingsIntervalModel IntervalB,
    Guid? Input,
    Guid? ResetInput,
    HttpTimerMode Mode
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

public enum HttpIntervalType
{
    Ticks,
    Hours,
    Days
}

public enum HttpTimerMode
{
    Delay,
    Pulse,
    Oscillator,
    Accumulator,
    Random
}
