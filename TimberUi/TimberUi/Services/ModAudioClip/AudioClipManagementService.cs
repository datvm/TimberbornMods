namespace TimberUi.Services;

public class AudioClipManagementService(
    AudioClipService audioClipService
)
{

    public IReadOnlyDictionary<string, AudioClip> AllAudioClips => audioClipService._audioClips;

    public AudioClip AddOrReplace(string filePath, string? name = default)
    {
        var audioClip = TimberUiUtils.LoadAudioClipFrom(filePath, name);
        AddOrReplace(audioClip.name, audioClip);
        return audioClip;
    }

    public void AddOrReplace(string name, AudioClip clip)
    {
        audioClipService._audioClips[name] = clip;
    }

    public bool Remove(string name) => audioClipService._audioClips.Remove(name);

}
