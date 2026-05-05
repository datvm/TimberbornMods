namespace TimberUi.CommonUi;

public readonly record struct ToggleGroupOption<TValue>(Toggle Toggle, TValue Value)
{
    public string Text => Toggle.text;
    public bool Selected => Toggle.value;
}

public class ToggleGroup<TValue>
{

    public ImmutableArray<ToggleGroupOption<TValue>> Options { get; }

    public event EventHandler<TValue>? OnValueChanged;

    public TValue Value => curr.Value;
    public ToggleGroupOption<TValue> SelectedOption => curr;

    ToggleGroupOption<TValue> curr;

    public ToggleGroup(IEnumerable<TValue> values, Func<TValue, Toggle> createToggleFunc)
        : this(values.Select(v => new ToggleGroupOption<TValue>(createToggleFunc(v), v))) { }

    public ToggleGroup(IEnumerable<ToggleGroupOption<TValue>> options)
    {
        Options = [.. options];

        foreach (var opt in Options)
        {
            var chk = opt.Toggle;
            chk.RegisterValueChangedCallback(e => OnToggleChanged(e.newValue, opt));
            chk.SetValueWithoutNotify(false);
        }

        if (Options.Length > 0)
        {
            InternalSwitchTo(Options[0]);
        }
    }

    void NotifyValueChanged() => OnValueChanged?.Invoke(this, Value);

    bool InternalSwitchTo(ToggleGroupOption<TValue> opt)
    {
        if (curr == opt) { return false; }

        if (curr != default)
        {
            curr.Toggle.SetValueWithoutNotify(false);
        }
        curr = opt;

        // This is needed in case no value is being selected
        curr.Toggle?.SetValueWithoutNotify(true);

        return true;
    }

    void OnToggleChanged(bool newValue, ToggleGroupOption<TValue> opt)
    {
        if (!newValue)
        {
            opt.Toggle.SetValueWithoutNotify(true);
            return;
        }

        if (InternalSwitchTo(opt)) // Most likely not needed but just in case
        {
            NotifyValueChanged();
        }
    }

    public void SetValue(TValue? value)
    {
        if (InternalFindAndSwitchTo(value))
        {
            NotifyValueChanged();
        }
    }

    public void SetValueWithoutNotify(TValue? value) => InternalFindAndSwitchTo(value);

    bool InternalFindAndSwitchTo(TValue? value)
    {
        var opt = FindOption(value);
        return InternalSwitchTo(opt);
    }

    public void ClearSelection() => SetValueWithoutNotify(default!);

    public ToggleGroupOption<TValue> FindOption(TValue? value) => Options.FirstOrDefault(o => Equals(o.Value, value));

    static bool Equals(TValue? a, TValue? b) => EqualityComparer<TValue?>.Default.Equals(a, b);

}
