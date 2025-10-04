using ModdableTimberborn.DependencyInjection.PrefabGroup;

namespace ModdableTimberborn.DependencyInjection.Patches;

[HarmonyPatch(typeof(PrefabGroupService)), HarmonyPatchCategory(ModdableDependencyInjectionConfig.PatchCategoryName)]
public static class PrefabGroupServicePatches
{

    [HarmonyPostfix, HarmonyPatch(nameof(PrefabGroupService.Load))]
    public static void OnAfterLoad(PrefabGroupService __instance)
    {
        ModdableTimberbornUtils.LogVerbose(() => $"{nameof(PrefabGroupService)} Load was called.");
        ContainerRetriever.GetInstance<PrefabGroupServiceTailRunnerService>().Run(__instance);
    }

}
