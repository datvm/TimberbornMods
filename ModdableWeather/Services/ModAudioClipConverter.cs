
namespace ModdableWeather.Services;

public class ModAudioClipConverter : IModFileConverter<AudioClip>
{
    public List<string> ValidExtensions { get; } = [".wav"];

    readonly List<AudioClip> audioClips = [];

    public ModAudioClipConverter()
    {
        Debug.Log("ModAudioClipConverter created");
    }

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
        Debug.Log("Trying audio: " + fileInfo.FullName);

        asset = WavUtility.ToAudioClip(fileInfo.FullName)
            ?? throw new InvalidDataException("Invalid WAV file: " + fileInfo.FullName);
        audioClips.Add(asset);

        Debug.Log(asset.name);

        return true;
    }
}
