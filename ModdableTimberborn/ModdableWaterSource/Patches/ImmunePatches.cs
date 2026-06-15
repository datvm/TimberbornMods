namespace ModdableTimberborn.ModdableWaterSource.Patches;

[HarmonyPatch, HarmonyPatchCategory(WaterSourceConfig.PatchCategoryName)]
public static class ImmunePatches
{

    [HarmonyPrefix, HarmonyPatch(typeof(DroughtWaterStrengthModifier), nameof(DroughtWaterStrengthModifier.GetStrengthModifier))]
    public static bool RemoveDroughtStrengthModifier(DroughtWaterStrengthModifier __instance, ref float __result)
    {
        if (__instance.GetComponent<ModdableWaterSourceComponent>().ImmuneToDrought.CalculateThisFrame())
        {
            __result = 1f;
            return false;
        }

        return true;
    }

    [HarmonyPrefix, HarmonyPatch(typeof(BadtideWaterSourceContaminationController), nameof(BadtideWaterSourceContaminationController.UpdateBadtideContamination))]
    public static bool RemoveBadtideContamination(BadtideWaterSourceContaminationController __instance)
    {
        if (__instance.GetComponent<ModdableWaterSourceComponent>().ImmuneToBadtide.CalculateThisFrame())
        {
            __instance._waterSourceContamination.ResetContamination();
            return false;
        }
        return true;
    }

}
