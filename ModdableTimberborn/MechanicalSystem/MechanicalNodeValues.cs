namespace ModdableTimberborn.MechanicalSystem;

public class MechanicalNodeValues
{
    public int NominalInput { get; set; }
    public int NominalOutput { get; set; }
}

public class ModdableMechanicalNodeValues(MechanicalNodeValues original) : ModdableValue<MechanicalNodeValues>(original)
{
}