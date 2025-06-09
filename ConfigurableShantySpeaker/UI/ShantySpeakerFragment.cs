namespace ConfigurableShantySpeaker.UI;

public class ShantySpeakerFragment(
    ILoc t,
    InputService inputService,
    VisualElementInitializer veInit,
    PanelStack panelStack,
    ShantySoundService shantySoundService
) : IEntityPanelFragment
{

#nullable disable
    GameSliderInt txtVolume;
    EntityPanelFragmentElement panel;
    VisualElement soundList;
    Toggle chkMute;
    VisualElement configPanel;
#nullable enable

    List<Toggle> chkSounds = [];
    ShantySpeakerConfigComponent? comp;

    public void ClearFragment()
    {
        panel.Visible = false;
        comp = null;
    }

    public VisualElement InitializeFragment()
    {
        panel = new() { Visible = false, };

        chkMute = panel.AddToggle(t.T("LV.CSS.Mute"), onValueChanged: ToggleMute)
            .SetMarginBottom(5);

        configPanel = panel.AddChild();

        txtVolume = configPanel.AddSliderInt(t.T("LV.CSS.Volume"), values: new(1, 100, 1))
            .AddEndLabel(v => v.ToString())
            .RegisterChange(OnVolumeChanged)
            .RegisterAlternativeManualValue(inputService, t, veInit, panelStack)
            .SetMarginBottom();

        var scrollView = configPanel.AddScrollView().SetMaxHeight(200);
        soundList = scrollView.AddChild();
        RefreshSoundList();

        AddButton("LV.CSS.OpenFolder", shantySoundService.OpenSoundsFolder);
        AddButton("LV.CSS.Refresh", RefreshMusicFolder);
        AddButton("LV.CSS.Reset", Reset);

        return panel.Initialize(veInit);

        void AddButton(string key, Action action)
        {
            configPanel.AddGameButton(t.T(key), action)
                .SetPadding(5).SetMarginBottom(5);
        }
    }

    void RefreshMusicFolder()
    {
        shantySoundService.Refresh();
        RefreshSoundList();
        UpdateContent();
    }

    void RefreshSoundList()
    {
        chkSounds = [];
        soundList.Clear();

        var curr = comp?.SoundName;

        foreach (var soundName in shantySoundService.SoundNames.Prepend(null))
        {
            Toggle toggle = null!;
            toggle = soundList.AddToggle(
                GetSoundDisplayName(soundName) ?? t.T("LV.CSS.DefaultShanty"),
                onValueChanged: (v) =>
                {
                    if (v)
                    {
                        SelectSound(soundName);
                    }
                    else
                    {
                        // Check itself again
                        toggle.SetValueWithoutNotify(true);
                    }
                })
                .SetMarginBottom(5);

            if (curr == soundName)
            {
                toggle.SetValueWithoutNotify(true);
            }

            toggle.dataSource = soundName;

            chkSounds.Add(toggle);
        }
    }

    public void ShowFragment(BaseComponent entity)
    {
        comp = entity.GetComponentFast<ShantySpeakerConfigComponent>();
        if (!comp) { return; }

        UpdateContent();
        panel.Visible = true;
    }

    void UpdateContent()
    {
        var stopped = comp!.Stopped;

        chkMute.SetValueWithoutNotify(stopped);
        configPanel.SetDisplay(!stopped);

        txtVolume.SetValueWithoutNotify(comp.Volume);
        SetSoundChecks();
    }

    public void UpdateFragment() { }

    void OnVolumeChanged(int volume)
    {
        comp?.SetVolume(volume);
    }

    void SetSoundChecks()
    {
        var name = comp?.SoundName;

        foreach (var chk in chkSounds)
        {
            chk.SetValueWithoutNotify((string?)chk.dataSource == name);
        }
    }

    void SelectSound(string? name)
    {
        if (!comp) { return; }

        comp.SetSoundName(name);
        SetSoundChecks();
    }

    void Reset()
    {
        comp?.ResetSpeaker();
        UpdateContent();
    }

    void ToggleMute(bool muted)
    {
        if (!comp) { return; }

        if (muted)
        {
            comp.Stop();
        }
        else
        {
            comp.Play();
        }
        UpdateContent();
    }

    static string? GetSoundDisplayName(string? name)
    {
        if (name is null) { return null; }

        var firstDot = name.IndexOf('.');
        return name[(firstDot + 1)..];
    }

}
