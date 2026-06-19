using Timberborn.Automation;

namespace ModdableTimberborn.BuildingSettings.BuiltInSettings;

// 1.1 reworked Relay from two fixed inputs (InputA/InputB) to a mode-dependent list of N inputs
// (List<AutomatorConnection> _inputs; SupportsMultipleInputs per mode). The setting model now carries
// a variable-length Guid array — one entry per input slot — instead of a fixed InputA/InputB pair.
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
    {
        var inputs = model.Inputs.Select(id => entityRegistry.DescribeEntity(id, t));
        var separator = $" {t.T("Building.Relay.Mode." + model.Mode)} ";

        return string.Join(separator, inputs);
    }

    protected override bool ApplyModel(RelaySettingsModel model, Relay target)
    {
        target.SetMode(model.Mode);

        // Grow toward the model's input count, but stop if IncreaseInputs() can't add another slot:
        // the current mode caps inputs (e.g. Not/Passthrough allow one), so SetInput(null, index>=1)
        // no-ops and the list never grows — without this no-progress break the loop would hang.
        while (target._inputs.Count < model.Inputs.Length)
        {
            var before = target._inputs.Count;
            target.IncreaseInputs();
            if (target._inputs.Count == before) { break; }
        }
        while (target._inputs.Count > model.Inputs.Length) { target.RemoveInput(target._inputs.Count - 1); }

        // Reconnect every existing slot. SetInput(null) is a no-op (Connect ignores null), so a cleared
        // input is disconnected explicitly. Slots beyond the model (mode couldn't shrink) are cleared.
        for (var i = 0; i < target._inputs.Count; i++)
        {
            var automator = i < model.Inputs.Length ? ResolveAutomator(model.Inputs[i]) : null;

            if (automator is not null) { target.SetInput(automator, i); }
            else { target._inputs[i].Disconnect(); }
        }

        target.Evaluate();

        return true;
    }

    protected override RelaySettingsModel GetModel(Relay target)
        => new(target.Mode, [.. target._inputs.Select(connection => connection.Transmitter?.GetEntityId())]);

    Automator? ResolveAutomator(Guid? entityId)
    {
        if (!entityId.HasValue) { return null; }

        var entity = entityRegistry.GetEntity(entityId.Value);
        return entity != null && entity.TryGetComponent<Automator>(out var automator) ? automator : null;
    }
}