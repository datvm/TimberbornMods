namespace TimberUi.Services;

public class ModAudioClipConverter : IModFileConverter<AudioClip>
{
    public List<string> ValidExtensions { get; } = [".wav"];

    readonly List<AudioClip> audioClips = [];

    public void Reset()
    {
        foreach (var aud in audioClips)
        {
            UnityEngine.Object.Destroy(aud);
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
        
        audioClips.Add(asset);

        return true;
    }
}
