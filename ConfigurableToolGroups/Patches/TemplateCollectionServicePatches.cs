namespace ConfigurableToolGroups.Patches;

[HarmonyPatch(typeof(TemplateCollectionService))]
public static class TemplateCollectionServicePatches
{

    [HarmonyPostfix, HarmonyPatch(nameof(TemplateCollectionService.Load))]
    public static void TailRun(TemplateCollectionService __instance)
    {
        ModdableToolGroupSpecService.Instance.Run(__instance);
    }

}
