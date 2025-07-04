namespace BeaverJukebox.Patches;

[HarmonyPatch(typeof(AudioClipService))]
public class MutePatches
{

    [HarmonyPrefix, HarmonyPatch(nameof(AudioClipService.GetAudioClipNames))]
    public static void PatchMutedNames(ref string soundName) => ReplaceWithMutedSoundName(ref soundName);

    [HarmonyPrefix, HarmonyPatch(nameof(AudioClipService.GetAudioClip))]
    public static void PatchMutedClip(ref string soundName) => ReplaceWithMutedSoundName(ref soundName);

    static void ReplaceWithMutedSoundName(ref string soundName)
    {
        Debug.Log($"For {soundName}: should mute? {AudioMuteService.Instance?.ShouldMute(soundName)}");

        if (AudioMuteService.Instance?.ShouldMute(soundName) == true)
        {
            soundName = AudioMuteService.MuteSoundName;
        }
    }

}
