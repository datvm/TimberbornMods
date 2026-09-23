namespace ScienceShop.Patches;

[HarmonyPatch(typeof(ManufactoryFragment))]
static class ManufactoryFragmentPatches
{
    [HarmonyPostfix, HarmonyPatch("get_Visible")]
    public static void HideForScienceShop(ManufactoryFragment __instance, ref bool __result)
    {
        if (!__result || !__instance._manufactory)
        {
            return;
        }

        if (__instance._manufactory.GetComponent<ConsumeScienceManufactory>())
        {
            __result = false;
        }
    }
}
