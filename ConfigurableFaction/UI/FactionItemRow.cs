namespace ConfigurableFaction.UI;

public class FactionItemRow<T> : VisualElement
{

    public T Data { get; private set; } = default!;
    Func<T, bool, SettingsFilter, bool>? additionalFilter;
    Func<T, bool, bool>? lockFunc;

    readonly Toggle chkEnabled;
    public event Action<T, bool>? OnValueChanged;

    public string Text { get; private set; } = "";

    public bool Value
    {
        get => chkEnabled.value;
        set
        {
            chkEnabled.SetValueWithoutNotify(value);
            CheckForLock();
        }
    }

    public bool Visible
    {
        get => this.IsDisplayed();
        set => this.SetDisplay(value);
    }

    public FactionItemRow()
    {
        chkEnabled = this.AddToggle(onValueChanged: OnCheckChanged).SetWidthPercent(100);
    }

    public FactionItemRow<T> SetItem(T spec, string text)
    {
        Data = spec;
        Text = chkEnabled.text = text;

        return this;
    }

    public FactionItemRow<T> SetAdditionalFilter(Func<T, bool, SettingsFilter, bool> filter)
    {
        additionalFilter = filter;
        return this;
    }

    public FactionItemRow<T> SetLockFunc(Func<T, bool, bool> func)
    {
        lockFunc = func;
        CheckForLock();

        return this;
    }

    public void Filter(SettingsFilter filter) => Visible =
        filter.Match(Text, Value)
        && (additionalFilter is null || additionalFilter(Data, Value, filter));

    public void CheckForLock()
    {
        enabledSelf = lockFunc is null || !lockFunc(Data, Value);
    }

    void OnCheckChanged(bool isChecked)
    {
        OnValueChanged?.Invoke(Data, isChecked);
        CheckForLock();
    }

}