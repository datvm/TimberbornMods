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

}
