namespace PackagerBuilder.Patches;

[HarmonyPatch(typeof(GoodGroupSpec))]
public static class GoodGroupPatches
{

    [HarmonyPrefix, HarmonyPatch(nameof(GoodGroupSpec.SingleResourceGroup), MethodType.Getter)]
    public static bool NoMoreSingleResourceGroup(ref bool __result)
    {
        __result = false;
        return false;
    }

}
