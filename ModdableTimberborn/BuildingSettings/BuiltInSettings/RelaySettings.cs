namespace ModdableTimberborn.BuildingSettings.BuiltInSettings;

public record RelaySettingsModel(
    RelayMode Mode,
    Guid? InputA,
    Guid? InputB
) : EntityIdModelBase([InputA, InputB])
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
}

public class RelaySettings(
    EntityRegistry entityRegistry,
    ILoc t
) : BuildingSettingsBase<Relay, RelaySettingsModel>(t)
{
    public override string DescribeModel(RelaySettingsModel model)
    {
        var inputA = entityRegistry.DescribeEntity(model.InputA, t);
        var inputB = entityRegistry.DescribeEntity(model.InputB, t);

        return $"{inputA} {t.T("Building.Relay.Mode." + model.Mode)} "
            + (model.Mode != RelayMode.Passthrough ? inputB : "");
    }

    protected override bool ApplyModel(RelaySettingsModel model, Relay target)
    {
        target.SetMode(model.Mode);

        target._inputA.TryConnecting(model.InputA, entityRegistry);
        target._inputB.TryConnecting(model.InputB, entityRegistry);

        target.Evaluate();

        return true;
    }

    protected override RelaySettingsModel GetModel(Relay target)
        => new(target.Mode, target.InputA?.GetEntityId(), target.InputB?.GetEntityId());
}