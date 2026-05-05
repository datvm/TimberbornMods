namespace TimberUi.CommonUi;

public class DropdownRow<TValue> : VisualElement
{

    public Label Label { get; }
    public Dropdown Dropdown { get; }

    public ImmutableArray<DropdownRowItem<TValue>> Items { get; private set; }
    public event EventHandler<IndexedDropdownRowItem<TValue>>? OnValueChanged;

    readonly DropdownRowProvider<TValue> provider = new();
    bool internalChanging;

    public DropdownRow()
    {
        this.SetAsRow().AlignItems();
        Label = this.AddLabel().SetMarginRight(5).SetFlexShrink(0).SetDisplay(false);

        provider.OnSelectedIndexChanged += OnItemChanged;

        Dropdown = this.AddDropdown().SetFlexGrow();
    }

    public DropdownRow(VisualElementInitializer veInit, DropdownItemsSetter dropdownItemsSetter) : this()
    {
        Initialize(veInit, dropdownItemsSetter);
    }

    public DropdownRow<TValue> Initialize(VisualElementInitializer veInit, DropdownItemsSetter dropdownItemsSetter)
    {
        Dropdown.Initialize(veInit);
        dropdownItemsSetter.SetItems(Dropdown, provider);

        return this;
    }

    void OnItemChanged(object sender, IndexedDropdownRowItem<TValue> e)
    {
        if (internalChanging) { return; }
        OnValueChanged?.Invoke(this, e);
    }

    public void SetLabel(string text)
    {
        Label.text = text;
        Label.SetDisplay(text.Length > 0);
    }

    public void SetItems(IEnumerable<TValue> values, Func<TValue, string> nameFunc, bool generateUniqueNames = false)
        => SetItems(values.Select(v => new DropdownRowItem<TValue>(v, nameFunc(v))), generateUniqueNames);

    public void SetItems(IEnumerable<DropdownRowItem<TValue>> values, bool generateUniqueNames = false)
    {
        var items = provider.Items;
        items.Clear();

        Dictionary<string, int> nameCount = [];
        foreach (var (v, text) in values)
        {
            var name = generateUniqueNames ? GetUniqueName(text) : text;
            items.Add(new(v, name));
        }

        string GetUniqueName(string name)
        {
            var count = nameCount.GetValueOrDefault(name);

            if (count == 0)
            {
                nameCount[name] = 1;
                return name;
            }

            var text = $"{name} ({count + 1})";
            nameCount[name] = count + 1;

            return GetUniqueName(text);
        }
    }

    public void SetSelectedValue(TValue value) => InternalSet(null, value);
    public void SetSelectedIndex(int index) => InternalSet(index, default);

    public void SetSelectedValueWithoutNotifying(TValue value)
    {
        internalChanging = true;
        InternalSet(null, value);
        internalChanging = false;
    }
    public void SetSelectedIndexWithoutNotifying(int index)
    {
        internalChanging = true;
        InternalSet(index, default);
        internalChanging = false;
    }

    internal void InternalSet(int? index, TValue? value)
    {
        if (index.HasValue)
        {
            provider.SetIndex(index.Value);
        }
        else if (value is not null)
        {
            provider.SetValue(value);
        }

        Dropdown.UpdateSelectedValue();
    }

}

public class DropdownRowProvider<TValue> : IDropdownProvider
{
    IReadOnlyList<string> IDropdownProvider.Items => [.. Items.Select(i => i.Text)];

    public event EventHandler<IndexedDropdownRowItem<TValue>>? OnSelectedIndexChanged;

    public List<DropdownRowItem<TValue>> Items { get; } = [];
    int currIndex;

    public IndexedDropdownRowItem<TValue> SelectedItem => new(currIndex, Items[currIndex]);
    public int SelectedIndex => currIndex;

    public string GetValue() => Items.Count == 0 ? "" : Items[currIndex].Text;

    public void SetValue(string value)
    {
        var oldIndex = currIndex;

        var index = Items.FindIndex(i => i.Text == value);
        index = currIndex = Math.Max(0, index);

        if (index != oldIndex)
        {
            OnSelectedIndexChanged?.Invoke(this, new(index, Items[index]));
        }
    }

    public void SetIndex(int index)
    {
        var oldIndex = currIndex;
        currIndex = Math.Max(0, Math.Min(Items.Count - 1, index));
        if (currIndex != oldIndex)
        {
            OnSelectedIndexChanged?.Invoke(this, new(currIndex, Items[currIndex]));
        }
    }

    public void SetValue(TValue value)
    {
        var index = Items.FindIndex(i => EqualityComparer<TValue>.Default.Equals(i.Value, value));
        SetIndex(index);
    }
}

public readonly record struct DropdownRowItem<TValue>(TValue Value, string Text);
public readonly record struct IndexedDropdownRowItem<TValue>(int Index, DropdownRowItem<TValue> Item);