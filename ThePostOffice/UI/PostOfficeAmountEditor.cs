namespace ThePostOffice.UI;

sealed class PostOfficeAmountEditor
{
    readonly IContainer container;
    readonly VisualElementInitializer veInit;
    readonly DropdownItemsSetter dropdowns;
    readonly ILoc t;
    readonly HashSet<string> known;
    readonly List<AutoRuleElement> lines = [];
    readonly Catalog catalog;

    VisualElement list = null!;
    bool updating;

    public event Action? Changed;

    public PostOfficeAmountEditor(
        IReadOnlyList<GoodSpec> specs,
        IGoodService goods,
        GoodDescriber describer,
        ILoc t,
        IContainer container,
        VisualElementInitializer veInit,
        DropdownItemsSetter dropdowns)
    {
        this.container = container;
        this.veInit = veInit;
        this.dropdowns = dropdowns;
        this.t = t;
        known = [];
        foreach (var spec in specs)
        {
            known.Add(spec.Id);
        }

        catalog = new(specs, goods, describer);
    }

    public void Build(VisualElement parent, Action insertBeforeRules, string help)
    {
        parent.AddGameLabel(t.T("LV.TPO.Good")).SetMarginBottom(5);
        var addRow = parent.AddRow().AlignItems().SetMarginBottom(8);
        if (catalog.Items.Count == 0)
        {
            addRow.SetDisplay(false);
        }
        else
        {
            var dropdown = new Dropdown().Initialize(veInit).SetFlexGrow().SetFlexShrink().SetMarginBottom(0);
            dropdown.style.minWidth = 0;
            dropdowns.SetItems(dropdown, catalog);
            addRow.Add(dropdown);
            addRow.AddGameButtonPadded(t.T("LV.TPO.Add"), AddSelected, paddingY: 3)
                .SetMargin(left: 5)
                .SetFlexShrink(0);
        }

        insertBeforeRules();
        parent.AddGameLabel(t.T("LV.TPO.InventoryRules").Bold()).SetMarginBottom(2);
        var helpLabel = parent.AddGameLabel(help).SetMarginBottom(5);
        helpLabel.style.whiteSpace = WhiteSpace.Normal;
        AutoRuleElement.AddHeaders(parent, t, t.T("LV.TPO.Amount"));
        list = parent.AddChild();
    }

    public void SetRules(IReadOnlyList<PostOfficeAutoRule> next)
    {
        updating = true;
        list.Clear();
        lines.Clear();
        foreach (var rule in next)
        {
            if (rule.Target <= 0 || !known.Contains(rule.GoodId) || lines.Exists(line => line.Id == rule.GoodId))
            {
                continue;
            }

            AddLine(rule.GoodId, rule.Target, Math.Max(0, rule.Margin));
        }

        updating = false;
    }

    public List<PostOfficeAutoRule> Read()
    {
        List<PostOfficeAutoRule> result = [];
        foreach (var line in lines)
        {
            if (line.Target <= 0)
            {
                continue;
            }

            result.Add(new(line.Id, line.Target, line.Margin));
        }

        return result;
    }

    void AddSelected()
    {
        if (updating || catalog.GetValue() is not { Length: > 0 } id)
        {
            return;
        }

        var existing = lines.Find(line => line.Id == id);
        if (existing is not null)
        {
            existing.SetTarget(existing.Target + 1);
            return;
        }

        AddLine(id, 1, 0);
        Notify();
    }

    void AddLine(string id, int target, int margin)
    {
        var line = container.GetInstance<AutoRuleElement>();
        line.Changed += Notify;
        line.Removed += () => RemoveLine(line);
        line.SetRule(id, target, margin);
        lines.Add(line);
        list.Add(line);
    }

    void RemoveLine(AutoRuleElement line)
    {
        if (updating)
        {
            return;
        }

        lines.Remove(line);
        line.RemoveFromHierarchy();
        Notify();
    }

    void Notify()
    {
        if (!updating)
        {
            Changed?.Invoke();
        }
    }

    public sealed class Catalog(IReadOnlyList<GoodSpec> specs, IGoodService goods, GoodDescriber describer) : IExtendedDropdownProvider
    {
        readonly List<string> ids = [..specs.Select(spec => spec.Id)];
        string selected = specs.Count == 0 ? "" : specs[0].Id;

        public IReadOnlyList<string> Items => ids;

        public string GetValue() => selected;

        public void SetValue(string value)
        {
            if (ids.Contains(value))
            {
                selected = value;
            }
        }

        public string FormatDisplayText(string value, bool chosen)
        {
            if (!goods.HasGood(value))
            {
                return value;
            }

            return goods.GetGood(value).DisplayName.Value;
        }

        public Sprite GetIcon(string value)
        {
            if (!goods.HasGood(value))
            {
                return null!;
            }

            return describer.GetIcon(value);
        }

        public ImmutableArray<string> GetItemClasses(string value) => [];
    }
}
