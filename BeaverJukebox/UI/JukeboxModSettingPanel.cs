namespace BeaverJukebox.UI;

public class JukeboxModSettingPanel : VisualElement
{
    readonly ModdableAudioClipService audioClipService;
    readonly AudioMuteService muter;
    readonly IExplorerOpener opener;
    readonly ILoc t;
    readonly ISystemFileDialogService filePickers;
    readonly DemoSoundPlayer soundPlayer;

    readonly VisualElement lstClips;
    readonly SoundListFilter filter = new();

    ImmutableArray<JukeboxItemElement> items = [];

    public JukeboxModSettingPanel(
        ModdableAudioClipService audioClipService,
        AudioMuteService muter,
        IExplorerOpener opener,
        ILoc t,
        VisualElementInitializer veInit,
        ISystemFileDialogService filePickers,
        DemoSoundPlayer soundPlayer
    )
    {
        this.audioClipService = audioClipService;
        this.muter = muter;
        this.opener = opener;
        this.t = t;
        this.filePickers = filePickers;
        this.soundPlayer = soundPlayer;

        var parent = this;

        var row = parent.AddRow().SetMarginBottom();

        row.AddGameButton(t.T("LV.BJb.StopAll"), onClick: soundPlayer.StopAll)
            .SetPadding(5).SetMarginRight()
            .SetMarginLeftAuto();
        row.AddGameButton(t.T("LV.BJb.OpenFolder"), onClick: OpenFolder)
            .SetPadding(5);

        AddFilterPanel(parent);
        
        lstClips = parent.AddChild();
        ReloadList();

        this.Initialize(veInit);
    }

    void AddFilterPanel(VisualElement parent)
    {
        var panel = parent.AddChild().SetMarginBottom();

        panel.AddGameLabel(t.T("LV.BJb.Filter").Bold());
        panel.AddTextField(changeCallback: content => UpdateFilter(f => f.Text = content));

        panel.AddToggle(t.T("LV.BJb.Muted"), onValueChanged: v => UpdateFilter(f => f.Muted = v));
        panel.AddToggle(t.T("LV.BJb.Replaced"), onValueChanged: v => UpdateFilter(f => f.Replaced = v));
    }

    void UpdateFilter(Action<SoundListFilter> update)
    {
        update(filter);

        foreach (var i in items)
        {
            i.Filter(filter);
        }
    }

    void ReloadList()
    {
        lstClips.Clear();
        List<JukeboxItemElement> items = [];

        foreach (var soundName in audioClipService.AllSoundNames)
        {
            var muted = muter.ShouldMute(soundName);
            AudioClipReplacement? replacement = null;

            if (!muted)
            {
                audioClipService.HasReplacement(soundName, out replacement);
            }

            var item = new JukeboxItemElement(soundName, muted, replacement, t);
            item.ActionRequested += action => ProcessRequest(soundName, action);
            item.Filter(filter);

            lstClips.Add(item);
            items.Add(item);
        }

        this.items = [.. items];
    }

    void ProcessRequest(string soundName, string action)
    {
        switch (action)
        {
            case "ToggleMute":
                muter.ToggleMuted(soundName);
                break;
            case "Replace":
                if (!AttemptReplace(soundName)) { return; }
                break;
            case "Restore":
                audioClipService.RemoveReplacement(soundName);
                break;
            case "Play":
                soundPlayer.Play(soundName);
                break;
            case "PlayOriginal":
                soundPlayer.PlayOriginal(soundName);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(action), action, null);
        }

        ReloadList();
    }

    bool AttemptReplace(string soundName)
    {
        var file = filePickers.ShowOpenFileDialog(".wav");
        if (string.IsNullOrEmpty(file)) { return false; }

        audioClipService.Replace(soundName, file);
        return true;
    }

    void OpenFolder()
    {
        opener.OpenDirectory(ModdableAudioClipService.WorkingFolder);
    }

}

public class SoundListFilter
{
    public string Text { get; set; } = "";
    public bool Muted { get; set; }
    public bool Replaced { get; set; }
}