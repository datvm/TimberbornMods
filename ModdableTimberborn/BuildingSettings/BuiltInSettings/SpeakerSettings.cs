namespace ModdableTimberborn.BuildingSettings.BuiltInSettings;

public record SpeakerSettingsModel(
    string? SoundId,
    SpeakerPlaybackMode PlaybackMode,
    SpeakerSpatialMode SpatialMode
);

public class SpeakerSettings(
    ILoc t,
    SpeakerSoundService speakerSoundService
) : BuildingSettingsBase<Speaker, SpeakerSettingsModel>(t)
{
    public override string DescribeModel(SpeakerSettingsModel model) 
        => model.SoundId ?? speakerSoundService.GetValidatedSoundId("");

    protected override bool ApplyModel(SpeakerSettingsModel model, Speaker target)
    {
        if (model.SoundId is not null)
        {
            target.SoundId = speakerSoundService.GetValidatedSoundId(model.SoundId);
        }
        target.PlaybackMode = model.PlaybackMode;
        target.SpatialMode = model.SpatialMode;

        target.StopAndPlayIfContinuous();

        return true;
    }

    protected override SpeakerSettingsModel GetModel(Speaker target)
        => new(target.SoundId, target.PlaybackMode, target.SpatialMode);
}