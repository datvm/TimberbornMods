namespace BeaverJukebox.UI;

public class JukeboxItemElement : VisualElement
{
    readonly string soundName;
    readonly bool isMuted;
    readonly AudioClipReplacement? replacement;

    public event Action<string> ActionRequested = null!;

    public JukeboxItemElement(string soundName, bool isMuted, AudioClipReplacement? replacement, ILoc t)
    {
        this.SetMarginBottom(10);

        this.AddLabel(soundName).SetFlexGrow();

        var row = this.AddRow().AlignItems();
        var lblStatus = row.AddGameLabel().SetFlexGrow();

        if (!isMuted)
        {
            AddGameButton("LV.BJb.Play", "Play");
        }

        AddGameButton("LV.BJb.ToggleMute", "ToggleMute");

        if (isMuted)
        {
            lblStatus.text = t.T("LV.BJb.Muted").Color(TimberbornTextColor.Red);
        }
        else
        {
            AddGameButton("LV.BJb.Replace", "Replace");

            if (replacement is not null)
            {
                lblStatus.text = t.T("LV.BJb.Replaced").Color(TimberbornTextColor.Green);
                AddGameButton("LV.BJb.PlayOriginal", "PlayOriginal");
                AddGameButton("LV.BJb.Restore", "Restore");
            }
        }

        void AddGameButton(string key, string action)
        {
            row.AddGameButton(t.T(key), () => ActionRequested(action))
                .SetPadding(5, 3)
                .SetMarginRight(5)
                .SetFlexShrink(0);
        }

        this.soundName = soundName;
        this.isMuted = isMuted;
        this.replacement = replacement;
    }

    public void Filter(SoundListFilter filter) => this.SetDisplay(ShouldShow(filter));

    bool ShouldShow(SoundListFilter filter)
    {
        if (filter.Muted && !isMuted) { return false; }
        if (filter.Replaced && replacement is null) { return false; }

        return string.IsNullOrEmpty(filter.Text) || soundName.Contains(filter.Text, StringComparison.OrdinalIgnoreCase);
    }

}
