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

        var currValue = MechanicalNodeValues.Value with { };
        modifiers.Modify(MechanicalNodeValues);

        if (currValue.Equals(MechanicalNodeValues.Value)) { return; }

        var c = OriginalComponent;
        c._nominalPowerInput = currValue.NominalInput;
        c._nominalPowerOutput = currValue.NominalOutput;
        OnMechanicalNodeValuesChanged?.Invoke(this, new ModdableValueChanged<MechanicalNodeValues>(currValue, MechanicalNodeValues.Value));
    }

}
