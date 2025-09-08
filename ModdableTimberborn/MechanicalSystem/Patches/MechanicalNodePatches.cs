namespace ModdableTimberborn.MechanicalSystem.Patches;

[HarmonyPatch]
public static class MechanicalNodePatches
{

    [HarmonyPostfix, HarmonyPatch(typeof(MechanicalNode), nameof(MechanicalNode.Awake))]
    public static void AwakePostfix(MechanicalNode __instance)
    {
        __instance.PatchAwakePostfix<MechanicalNode, ModdableMechanicalNode>();
    }

}
