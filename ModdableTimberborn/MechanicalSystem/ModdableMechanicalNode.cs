namespace ModdableTimberborn.MechanicalSystem;

public class ModdableMechanicalNode : BaseModdableComponent<MechanicalNode>, IModdableComponentAwake
{

#nullable disable
    MechanicalNodeModifierCollection modifiers;
    public ModdableMechanicalNodeValues MechanicalNodeValues { get; private set; }
#nullable enable

    public void AwakeAfter()
    {
        modifiers = new(this);

        var original = OriginalComponent;
        MechanicalNodeValues = new(new()
        {
            NominalInput = original._nominalPowerInput,
            NominalOutput = original._nominalPowerOutput,
        });
    }

    public void UpdateInput()
    {
        if (modifiers.IsDirty)
        {   
            modifiers.Modify(MechanicalNodeValues);
        }
    }

    
}
