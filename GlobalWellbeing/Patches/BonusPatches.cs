
namespace GlobalWellbeing.Patches;

[HarmonyPatch]
public static class BonusPatches
{

    [HarmonyPrefix, HarmonyPatch(typeof(WellbeingTierManager), nameof(WellbeingTierManager.UpdateBonuses))]
    public static bool PatchWellbeingUpdateBonuses(WellbeingTierManager __instance) => ShouldNotPatch(__instance);

    [HarmonyPrefix, HarmonyPatch(typeof(WellbeingTierManager), nameof(WellbeingTierManager.AddBonus))]
    public static bool PatchWellbeingAddBonus(WellbeingTierManager __instance) => ShouldNotPatch(__instance);

    [HarmonyPrefix, HarmonyPatch(typeof(WellbeingTierManager), nameof(WellbeingTierManager.RemoveBonus))]
    public static bool PatchWellbeingRemoveBonus(WellbeingTierManager __instance) => ShouldNotPatch(__instance);

    static bool ShouldNotPatch(WellbeingTierManager c) =>
        c.GetComponentFast<BeaverSpec>() is null &&
        c.GetComponentFast<Child>() is null;

}
