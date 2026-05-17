namespace MoreHttpApi.Shared.BuildingSettings;

public record SpeakerSettingsModel(
    string? SoundId,
    HttpSpeakerPlaybackMode PlaybackMode,
    HttpSpeakerSpatialMode SpatialMode
);

public enum HttpSpeakerPlaybackMode
{
    Once,
    Continuously
}

public enum HttpSpeakerSpatialMode
{
    Spatial,
    NonSpatial
}
