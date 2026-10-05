namespace ConveyorBelt.UI;

[BindFragment]
public class SmartSplitterFragment(
    ILoc t,
    BeltGoodService goods,
    IGoodService goodService,
    GoodDescriber describer,
    VisualElementInitializer veInit,
    DropdownItemsSetter dropdownItemsSetter
) : BaseEntityPanelFragment<SmartSplitter>
{
    readonly SplitterGoodList[] lists = new SplitterGoodList[3];
    readonly Dropdown[] dropdowns = new Dropdown[3];
    bool refreshing;

    protected override void InitializePanel()
    {
        List<GoodSpec> carryable = [.. goods.CarryableGoods()];
        carryable.Sort((a, b) => string.Compare(
            a.DisplayName.Value,
            b.DisplayName.Value,
            StringComparison.CurrentCultureIgnoreCase));

        List<string> ids = ["-", "", "*"];
        foreach (var good in carryable)
        {
            ids.Add(good.Id);
        }

        (string Key, string Arrow, Color Color)[] headings =
        [
            ("LV.CBlt.Left", "\u2190", SplitterMarks.Left),
            ("LV.CBlt.Center", "\u2191", SplitterMarks.Center),
            ("LV.CBlt.Right", "\u2192", SplitterMarks.Right),
        ];

        for (var i = 0; i < headings.Length; i++)
        {
            var index = i;
            var heading = headings[i];
            var row = panel.AddRow().AlignItems().SetMarginBottom(2);
            var title = row.AddLabel(t.T(heading.Key)).SetFlexGrow();
            title.style.color = Color.white;
            var arrow = row.AddLabel(heading.Arrow);
            arrow.style.color = heading.Color;
            arrow.style.fontSize = 16;
            arrow.style.unityFontStyleAndWeight = FontStyle.Bold;
            arrow.style.flexShrink = 0;

            var list = new SplitterGoodList(ids, goodService, describer, t);
            list.Changed += value => OnPort(index, value);
            var dropdown = new Dropdown().Initialize(veInit).SetMarginBottom();
            dropdownItemsSetter.SetItems(dropdown, list);
            panel.Add(dropdown);
            lists[i] = list;
            dropdowns[i] = dropdown;
        }
    }

    public override void ShowFragment(BaseComponent entity)
    {
        base.ShowFragment(entity);
        Refresh();
    }

    public override void UpdateFragment() => Refresh();

    void Refresh()
    {
        if (component is not SmartSplitter splitter)
        {
            panel.Visible = false;
            return;
        }

        refreshing = true;
        panel.Visible = true;
        var ports = splitter.SplitPorts;
        for (var i = 0; i < lists.Length && i < ports.Count; i++)
        {
            lists[i].Select(ports[i].Serialize());
            dropdowns[i].UpdateSelectedValue();
        }

        refreshing = false;
    }

    void OnPort(int index, string value)
    {
        if (refreshing || component is not SmartSplitter splitter)
        {
            return;
        }

        splitter.SetSplitPort(index, SmartSplitPort.Deserialize(value));
    }

    sealed class SplitterGoodList(IReadOnlyList<string> ids, IGoodService goods, GoodDescriber describer, ILoc t)
        : IExtendedDropdownProvider
    {
        string selected = "";

        public event Action<string>? Changed;

        public IReadOnlyList<string> Items => ids;

        public string GetValue() => selected;

        public void Select(string value) => selected = value;

        public void SetValue(string value)
        {
            if (selected == value)
            {
                return;
            }

            selected = value;
            Changed?.Invoke(value);
        }

        public string FormatDisplayText(string value, bool selected)
        {
            if (value == "-")
            {
                return t.T("LV.CBlt.None");
            }

            if (value.Length == 0)
            {
                return t.T("LV.CBlt.Any");
            }

            if (value == "*")
            {
                return t.T("LV.CBlt.Overflow");
            }

            return goods.GetGood(value).DisplayName.Value;
        }

        public Sprite GetIcon(string value)
        {
            if (value is "-" or "*" || value.Length == 0 || !goods.HasGood(value))
            {
                return null!;
            }

            return describer.GetIcon(value);
        }

        public ImmutableArray<string> GetItemClasses(string value) => [];
    }
}
