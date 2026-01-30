namespace XtraPlzNoCrashes.Patches;

[HarmonyPatch(typeof(NeedVerifier))]
public static class NeedVerifierPatches
{

    [HarmonyPrefix, HarmonyPatch(nameof(NeedVerifier.VerifyAllCollectionsAreUsed))]
    public static bool SkipAllNeedsCollectionVerifier() => false;

    [HarmonyPrefix, HarmonyPatch(nameof(NeedVerifier.VerifyAllNeedsAreUsed))]
    public static bool SkipAllNeedsVerifier() => false;

}
