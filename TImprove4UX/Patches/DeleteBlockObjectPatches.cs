using Timberborn.BlockObjectTools;

namespace TImprove4UX.Patches;

[HarmonyPatch]
public static class DeleteBlockObjectPatches
{

    [HarmonyPrefix, HarmonyPatch(typeof(BlockObjectTool), nameof(BlockObjectTool.ProcessInput))]
    public static bool PatchBlockObjTool(BlockObjectTool __instance, ref bool __result)
    {
        if (AlternateDeleteObjectTool.Instance?.InterceptToolInput(__instance) == true)
        {
            __result = true;
            return false;
        }

        return true;
    }

    [HarmonyPrefix, HarmonyPatch(typeof(BuildingDeconstructionTool), nameof(BuildingDeconstructionTool.FillObjectsToDeconstruct))]
    public static void FilterDeconstructObjectsForPreview(BuildingDeconstructionTool __instance, ref IEnumerable<BlockObject> blockObjects)
        => FilterDeconstructObjects(__instance, ref blockObjects);

    [HarmonyPrefix, HarmonyPatch(typeof(BlockObjectDeletionTool<BuildingSpec>), nameof(BuildingDeconstructionTool.ActionCallback))]
    public static void FilterDeconstructObjectsForAction(object __instance, ref IEnumerable<BlockObject> blockObjects)
        => FilterDeconstructObjects(__instance, ref blockObjects);

    static void FilterDeconstructObjects(object __instance, ref IEnumerable<BlockObject> blockObjects)
    {
        if (__instance is not AlternateDeleteObjectTool altTool) { return; }
        altTool.FilterObjectsToDeconstruct(ref blockObjects);
    }

    [HarmonyPrefix, HarmonyPatch(typeof(BlockObjectDeletionTool<BuildingSpec>), nameof(BuildingDeconstructionTool.ProcessInput))]
    public static bool CheckForAlternateInput(object __instance, ref bool __result)
    {
        if (__instance is AlternateDeleteObjectTool altTool
            && altTool.AltToolProcessInput())
        {
            __result = true;
            return false;
        }

        return true;
    }

}
