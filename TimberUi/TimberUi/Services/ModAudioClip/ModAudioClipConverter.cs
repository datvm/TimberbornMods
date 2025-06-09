namespace TimberUi.Services;

public class ModAudioClipConverter : IModFileConverter<AudioClip>
{
    public List<string> ValidExtensions { get; } = [".wav"];

    readonly List<ModAudioClip> audioClips = [];
    public IReadOnlyList<ModAudioClip> AudioClips => audioClips.AsReadOnly();

    public void Reset()
    {
        foreach (var aud in audioClips)
        {
            UnityEngine.Object.Destroy(aud.AudioClip);
        }
        audioClips.Clear();
    }

    public bool TryConvert(FileInfo fileInfo, SerializedObject metadata, out AudioClip asset)
    {
        try
        {
            asset = WavUtility.ToAudioClip(fileInfo.FullName);
        }
        catch (Exception ex)
        {
            throw new InvalidDataException("Invalid WAV file: " + fileInfo.FullName, ex);
        }
        
        audioClips.Add(new(fileInfo.FullName, asset));

        return true;
    }
}

public readonly record struct ModAudioClip(string FullName, AudioClip AudioClip);