namespace TimberUi.Services;

public class ModAudioClipConverter : IModFileConverter<AudioClip>
{
    public static readonly FrozenSet<string> ValidExtensions = [".wav"];

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

    public bool CanConvert(FileInfo fileInfo) => ValidExtensions.Contains(fileInfo.Extension.ToLower());

    public bool TryConvert(OrderedFile orderedFile, string path, SerializedObject metadata, out AudioClip asset)
    {
        var fileInfo = orderedFile.File;
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