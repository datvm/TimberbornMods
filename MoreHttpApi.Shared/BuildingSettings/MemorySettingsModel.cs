namespace MoreHttpApi.Shared.BuildingSettings;

public record MemorySettingsModel(
    HttpMemoryMode Mode,
    Guid? InputA,
    Guid? InputB,
    Guid? ResetInput
) : EntityIdModelBase([InputA, InputB, ResetInput])
{
    public Guid? InputA
    {
        get => EntityIds[0];
        set => EntityIds[0] = value;
    }

    public Guid? InputB
    {
        get => EntityIds[1];
        set => EntityIds[1] = value;
    }

    public Guid? ResetInput
    {
        get => EntityIds[2];
        set => EntityIds[2] = value;
    }
}

public enum HttpMemoryMode
{
    SetReset,
    Toggle,
    Latch,
    FlipFlop
}
