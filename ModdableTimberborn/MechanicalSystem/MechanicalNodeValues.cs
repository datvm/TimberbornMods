namespace ModdableTimberborn.MechanicalSystem;

public record MechanicalNodeValues(int NominalInput, int NominalOutput);

public class ModdableMechanicalNodeValues(MechanicalNodeValues original) : ModdableValue<MechanicalNodeValues>(original)
{
}