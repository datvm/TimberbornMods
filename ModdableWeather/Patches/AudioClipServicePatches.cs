namespace ModdableWeather.Patches;

[HarmonyPatch(typeof(AudioClipService))]
public static class AudioClipServicePatches
{
    const string ModdedSoundsDirectoryKey = "resources/sounds";

    [HarmonyPostfix, HarmonyPatch(nameof(AudioClipService.LoadAudioClips))]
    public static void AddModdedSounds(AudioClipService __instance)
    {
        var items = __instance._assetLoader.LoadAll<AudioClip>(ModdedSoundsDirectoryKey).ToArray();
        Debug.Log($"{items.Length} modded AudioClip");

        foreach (var item in items)
        {
            __instance._audioClips[item.Asset.name] = item.Asset;
        }
    }

}
