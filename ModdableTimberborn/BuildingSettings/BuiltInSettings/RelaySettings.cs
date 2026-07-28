namespace ModdableTimberborn.BuildingSettings.BuiltInSettings;

public record RelaySettingsModel(
    RelayMode Mode,
    Guid?[] Inputs
) : EntityIdModelBase(Inputs);

public class RelaySettings(
    EntityRegistry entityRegistry,
    ILoc t
) : EntityIdBuildingSettingsBase<Relay, RelaySettingsModel>(t)
{
    public override string DescribeModel(RelaySettingsModel model) 
        => $"{t.T("Building.Relay.Mode." + model.Mode)} x{model.Inputs.Length}";

    protected override bool ApplyModel(RelaySettingsModel model, Relay target)
    {
        target.SetMode(model.Mode);

        target._inputs.Clear();
        foreach (var input in model.Inputs)
        {
            // No need to check for null, adding a null is now valid for Relay.
            target.AddAndConnect(entityRegistry.TryGetAutomator(input));
        }

        target.Evaluate();

        return true;
    }

    protected override RelaySettingsModel GetModel(Relay target)
        => new(target.Mode, [..target.Inputs.Select(i => i.Transmitter?.GetEntityId())]);
}