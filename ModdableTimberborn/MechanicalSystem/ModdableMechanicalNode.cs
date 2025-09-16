namespace ModdableTimberborn.MechanicalSystem;

public class ModdableMechanicalNode : BaseModdableComponent<MechanicalNode>, IModdableComponentAwake
{

#nullable disable
    MechanicalNodeModifierCollection modifiers;
    public ModdableMechanicalNodeValues MechanicalNodeValues { get; private set; }
#nullable enable

    public event EventHandler<ModdableValueChanged<MechanicalNodeValues>>? OnMechanicalNodeValuesChanged;

    public void AwakeAfter()
    {
        modifiers = new(this);

        var original = OriginalComponent;
        MechanicalNodeValues = new(new(original._nominalPowerInput, original._nominalPowerOutput));
    }

    public void UpdateValues()
    {
        if (!modifiers.IsDirty) { return; }

        var prev = MechanicalNodeValues.Value with { };
        if (!modifiers.Modify(MechanicalNodeValues)) { return; }

        var curr = MechanicalNodeValues.Value;
        var c = OriginalComponent;
        c._nominalPowerInput = curr.NominalInput;
        c._nominalPowerOutput = curr.NominalOutput;

        OnMechanicalNodeValuesChanged?.Invoke(this, new ModdableValueChanged<MechanicalNodeValues>(prev, curr));
    }

}
