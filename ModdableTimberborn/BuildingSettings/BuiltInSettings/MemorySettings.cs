namespace ModdableTimberborn.BuildingSettings.BuiltInSettings;

public record MemorySettingsModel(
    MemoryMode Mode,
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

public class MemorySettings(
    ILoc t,
    EntityRegistry entityRegistry
) : BuildingSettingsBase<Memory, MemorySettingsModel>(t)
{
    public override string DescribeModel(MemorySettingsModel model) => "";

    protected override bool ApplyModel(MemorySettingsModel model, Memory target)
    {
        target.SetMode(model.Mode);

        target._inputA.TryConnecting(model.InputA, entityRegistry);
        target._inputB.TryConnecting(model.InputB, entityRegistry);
        target._resetInput.TryConnecting(model.ResetInput, entityRegistry);

        return true;
    }

    protected override MemorySettingsModel GetModel(Memory target)
        => new(target.Mode, target.InputA?.GetEntityId(), target.InputB?.GetEntityId(), target.ResetInput?.GetEntityId());
}