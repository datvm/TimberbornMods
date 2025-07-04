namespace BeaverJukebox;

public class MSettings(
    ISettings settings,
    ModSettingsOwnerRegistry modSettingsOwnerRegistry,
    ModRepository modRepository,
    AudioMuteService muter,
    ModdableAudioClipService audioClipService
) : ModSettingsOwner(settings, modSettingsOwnerRegistry, modRepository)
{

    public override string ModId { get; } = nameof(BeaverJukebox);

    public JukeboxModSetting Jukebox { get; } = new();

    public override void OnAfterLoad()
    {
        Jukebox.OnResetRequested += Jukebox_OnResetRequested;
    }

    private void Jukebox_OnResetRequested()
    {
        muter.ClearMuted();
        audioClipService.ClearReplacement();
    }
}
