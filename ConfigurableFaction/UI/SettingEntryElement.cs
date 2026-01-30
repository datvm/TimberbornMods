namespace ConfigurableFaction.UI;

public abstract class SettingEntryElement : VisualElement
{
    public EffectiveEntry Entry { get; }
    protected Toggle Toggle { get; set; } = null!;
    public string Keyword { get; protected set; }

    public event Action<bool> OnToggled = null!;

    public SettingEntryElement(EffectiveEntry entry)
    {
        Entry = entry;

        this.SetAsRow().AlignItems();
        Toggle = this.AddToggle(onValueChanged: v => OnToggled(v));
        UpdateEntryState();
    }

    public void UpdateEntryState()
    {
        Toggle.SetValueWithoutNotify(Entry.Checked);
        enabledSelf = !Entry.Locked;
    }

    public void SetFilter(string keyword)
        => this.SetDisplay(keyword.Length == 0 || Keyword.Contains(keyword));

}
