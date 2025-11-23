namespace RealStars.Patches;

[HarmonyPatch]
public static class SkyBoxPatches
{

    [HarmonyPrefix, HarmonyPatch(typeof(SkyboxComponent), nameof(SkyboxComponent.Start))]
    public static void Test(SkyboxComponent __instance)
    {
        Debug.Log("Skybox Started: " + __instance);
    }

}
