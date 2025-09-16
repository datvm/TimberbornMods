namespace ModdableTimberborn.MechanicalSystem.Patches;

[HarmonyPatchCategory(ModdableMechanicalSystemConfigurator.PatchCategoryName), HarmonyPatch(typeof(NoPowerStatus))]
public static class NoPowerStatusPatches
{

    [HarmonyPrefix, HarmonyPatch(nameof(NoPowerStatus.CanPotentiallyBePowered))]
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
