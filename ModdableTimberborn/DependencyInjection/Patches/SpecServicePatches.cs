using ModdableTimberborn.DependencyInjection.Specs;

namespace ModdableTimberborn.DependencyInjection.Patches;

[HarmonyPatch(typeof(SpecService)), HarmonyPatchCategory(ModdableDependencyInjectionConfig.PatchCategoryName)]
public static class SpecServicePatches
{

    [HarmonyPostfix, HarmonyPatch(nameof(SpecService.Load))]
    public static void LoadPostfix(SpecService __instance)
    {
        ModdableTimberbornUtils.LogVerbose(() => "SpecService.Load finished.");

        var runner = ContainerRetriever.Container.GetInstance<SpecServiceRunner>();
        runner.OnSpecLoaded(__instance);
    }

}
