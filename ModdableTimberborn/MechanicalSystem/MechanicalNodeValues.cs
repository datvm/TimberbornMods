namespace ModdableTimberborn.MechanicalSystem;

public readonly record struct MechanicalNodeValues(int NominalInput, int NominalOutput);

public class ModdableMechanicalNodeValues(MechanicalNodeValues original) : ModdableValue<MechanicalNodeValues>(original)
{
}