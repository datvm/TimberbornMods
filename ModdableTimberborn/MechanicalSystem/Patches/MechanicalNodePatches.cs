namespace ModdableTimberborn.MechanicalSystem.Patches;

[HarmonyPatchCategory(ModdableMechanicalSystemConfig.PatchCategoryName), HarmonyPatch(typeof(MechanicalNode))]
public static class MechanicalNodePatches
{

    [HarmonyPostfix, HarmonyPatch(nameof(MechanicalNode.Awake))]
    public static void AwakePostfix(MechanicalNode __instance)
    {
        __instance.PatchAwakePostfix<MechanicalNode, ModdableMechanicalNode>();
    }

    [HarmonyPrefix, HarmonyPatch(nameof(MechanicalNode.CanPotentiallyBePowered))]
    public static bool PatchWhenZeroUsage(NoPowerStatus __instance, ref bool __result)
    {
        if (__instance._mechanicalNode._nominalPowerInput <= 0)
        {
            __result = true;
            return false;
        }

        return true;
    }

}
