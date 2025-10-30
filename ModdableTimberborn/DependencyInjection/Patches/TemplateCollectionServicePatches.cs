namespace ModdableTimberborn.DependencyInjection.Patches;

[HarmonyPatch(typeof(TemplateCollectionService)), HarmonyPatchCategory(ModdableDependencyInjectionConfig.PatchCategoryName)]
public static class TemplateCollectionServicePatches
{

    [HarmonyPostfix, HarmonyPatch(nameof(TemplateCollectionService.Load))]
    public static void OnAfterLoad(TemplateCollectionService __instance)
    {
        ContainerRetriever.GetInstance<TemplateCollectionTailRunnerService>().Run(__instance);
    }

}
