global using Timberborn.MechanicalSystem;

namespace LateGamePower;

[HarmonyPatch(typeof(MechanicalNode), nameof(MechanicalNode.UpdateOutput))]
public static class MechanicalNodePatch
{

    public static void Prefix(ref float output)
    {
        var mul = ScienceToPowerService.Instance?.CurrentMultiplication ?? 0;
        if (mul <= 1) { return; }

        output *= mul;
    }

}
