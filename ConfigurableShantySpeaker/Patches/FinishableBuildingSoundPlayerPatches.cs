namespace ConfigurableShantySpeaker.Patches;

[HarmonyPatch(typeof(FinishableBuildingSoundPlayer))]
public static class FinishableBuildingSoundPlayerPatches
{

    [HarmonyPrefix, HarmonyPatch(nameof(FinishableBuildingSoundPlayer.Awake))]
    public static bool SkipAwake() => false;

    [HarmonyPrefix, HarmonyPatch(nameof(FinishableBuildingSoundPlayer.OnEnterFinishedState))]
    public static bool SkipOnEnterFinishedState() => false;

    [HarmonyPrefix, HarmonyPatch(nameof(FinishableBuildingSoundPlayer.OnExitFinishedState))]
    public static bool SkipOnExitFinishedState() => false;

    //[HarmonyPostfix, HarmonyPatch(typeof(AudioClipService), nameof(AudioClipService.LoadAudioClips))]
    //public static void Test(AudioClipService __instance)
    //{
    //    Debug.Log(string.Join(Environment.NewLine, __instance._audioClips.Keys));
    //}

}
