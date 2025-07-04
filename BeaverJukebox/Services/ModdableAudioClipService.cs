namespace BeaverJukebox.Services;

public class ModdableAudioClipService(IAssetLoader assetLoader) : AudioClipService(assetLoader), ILoadableSingleton
{
    public static readonly string WorkingFolder = Path.Combine(UserDataFolder.Folder, nameof(BeaverJukebox));
    public const string OriginalSoundPostfix = "|Original";

    readonly Dictionary<string, AudioClipReplacement> replacements = [];
    public IEnumerable<string> AllSoundNames => _audioClips.Keys;

    public void Load()
    {
        Directory.CreateDirectory(WorkingFolder);
    }

    public void OverrideAudios()
    {
        replacements.Clear();
        var files = Directory.GetFiles(WorkingFolder, "*.wav", SearchOption.TopDirectoryOnly);

        foreach (var file in files)
        {
            OverrideAudio(file);
        }

        // Add silent
        AddSilentFile();
    }

    void OverrideAudio(string file)
    {
        var name = Path.GetFileNameWithoutExtension(file);

        // Check for existing replacement
        AudioClip originalClip;

        if (replacements.TryGetValue(name, out var replacement))
        {
            originalClip = replacement.OriginalClip;
        }
        else if (!_audioClips.TryGetValue(name, out originalClip))
        {
            Debug.LogWarning($"Audio clip '{name}' not found in the original audio clips. Skipping.");
            return;
        }

        var audioClip = TimberUiUtils.LoadAudioClipFrom(file, name);
        _audioClips[name] = audioClip;
        replacements[name] = new(name, originalClip);
    }

    void AddSilentFile()
    {
        var path = Path.Combine(MStarter.ModPath, "Resources/Silent.wav");
        var clip = TimberUiUtils.LoadAudioClipFrom(path, AudioMuteService.MuteSoundName);
        _audioClips[AudioMuteService.MuteSoundName] = clip;
    }

    public bool NewGetAudioClip(string soundName, ref AudioClip __result)
    {
        if (soundName.EndsWith(OriginalSoundPostfix))
        {
            var properName = soundName[..^OriginalSoundPostfix.Length];

            if (!HasReplacement(properName, out var replacement))
            {
                throw new ArgumentException($"No replacement found for sound '{soundName}'");
            }

            __result = replacement.OriginalClip;
            return false;
        }

        return true;
    }

    public bool NewGetAudioClipNames(string soundName, ref IEnumerable<string> __result)
    {
        if (soundName.EndsWith(OriginalSoundPostfix))
        {
            var properName = soundName[..^OriginalSoundPostfix.Length];
            if (!HasReplacement(properName, out var replacement))
            {
                throw new ArgumentException($"No replacement found for sound '{soundName}'");
            }

            __result = GetAudioClipNames(properName)
                .Select(q => q + OriginalSoundPostfix);            
            return false;
        }

        return true;
    }

    public bool HasReplacement(string name, [NotNullWhen(true)] out AudioClipReplacement? replacement)
        => replacements.TryGetValue(name, out replacement);

    public void Replace(string soundName, string filePath)
    {
        var target = GetOverrideFile(soundName);
        File.Copy(filePath, target, true);

        OverrideAudio(target);
    }

    public void RemoveReplacement(string soundName)
    {
        if (replacements.TryGetValue(soundName, out var replacement))
        {
            _audioClips[soundName] = replacement.OriginalClip;
            replacements.Remove(soundName);
        }

        var filePath = GetOverrideFile(soundName);
        if (File.Exists(filePath))
        {
            File.Delete(filePath);
        }
    }

    public void ClearReplacement()
    {
        var files = GetOverrideFiles();
        foreach (var file in files)
        {
            File.Delete(file);
        }

        foreach (var (name, r) in replacements)
        {
            _audioClips[name] = r.OriginalClip;
        }
        replacements.Clear();
    }

    static string[] GetOverrideFiles() => Directory.GetFiles(WorkingFolder, "*.wav", SearchOption.TopDirectoryOnly);
    static string GetOverrideFile(string soundName) => Path.Combine(WorkingFolder, $"{soundName}.wav");

}

public record AudioClipReplacement(string SoundName, AudioClip OriginalClip);

[HarmonyPatch(typeof(AudioClipService))]
public static class AudioClipServiceRedirect
{

    [HarmonyPostfix, HarmonyPatch(nameof(AudioClipService.LoadAudioClips))]
    public static void OverrideAfterLoad(ModdableAudioClipService __instance)
    {
        __instance.OverrideAudios();
    }

    [HarmonyPrefix, HarmonyPatch(nameof(AudioClipService.GetAudioClip))]
    public static bool PatchGetAudioClip(ModdableAudioClipService __instance, string soundName, ref AudioClip __result) 
        => __instance.NewGetAudioClip(soundName, ref __result);

    [HarmonyPrefix, HarmonyPatch(nameof(AudioClipService.GetAudioClipNames))]
    public static bool PatchGetAudioClipNames(ModdableAudioClipService __instance, string soundName, ref IEnumerable<string> __result) 
        => __instance.NewGetAudioClipNames(soundName, ref __result);

}


