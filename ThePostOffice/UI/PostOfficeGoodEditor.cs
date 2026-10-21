namespace ThePostOffice.UI;

sealed class PostOfficeGoodEditor : VisualElement
{
    const int MaxAmount = 9999;

    readonly IGoodService goods;
    readonly VisualElementInitializer veInit;
    readonly HashSet<string> known;
    readonly List<Line> lines = [];
    readonly List<GoodAmount> hidden = [];
    readonly VisualElement list;
    readonly GoodList catalog;

    bool updating;

    public event Action? Changed;

    public PostOfficeGoodEditor(
        IReadOnlyList<GoodSpec> goods,
        IGoodService goodService,
        GoodDescriber describer,
        ILoc t,
        VisualElementInitializer veInit,
        DropdownItemsSetter dropdowns)
    {
        this.goods = goodService;
        this.veInit = veInit;
        known = [];
        foreach (var spec in goods)
        {
            known.Add(spec.Id);
        }

        list = this.AddChild();
        var addRow = this.AddRow().AlignItems();
        var dropdown = new Dropdown().Initialize(veInit).SetFlexGrow().SetFlexShrink().SetMarginBottom(0);
        dropdown.style.minWidth = 0;
        catalog = new(goods, goodService, describer);
        if (catalog.Items.Count == 0)
        {
            addRow.SetDisplay(false);
        }
        else
        {
            dropdowns.SetItems(dropdown, catalog);
            addRow.Add(dropdown);
        }

        addRow.AddGameButtonPadded(t.T("LV.TPO.AddGood"), AddSelected, paddingY: 3)
            .SetMargin(left: 5)
            .SetFlexShrink(0);

        this.Initialize(veInit);
    }

    public void SetGoods(IReadOnlyList<GoodAmount> next)
    {
        updating = true;
        list.Clear();
        lines.Clear();
        hidden.Clear();

        Dictionary<string, int> merged = [];
        List<string> order = [];
        foreach (var good in next)
        {
            if (good.Amount <= 0)
            {
                continue;
            }

            if (!known.Contains(good.GoodId))
            {
                hidden.Add(good);
                continue;
            }

            if (!merged.ContainsKey(good.GoodId))
            {
                order.Add(good.GoodId);
            }

            merged[good.GoodId] = merged.GetValueOrDefault(good.GoodId) + good.Amount;
        }

        foreach (var id in order)
        {
            AddLine(id, merged[id]);
        }

        updating = false;
    }

    public List<GoodAmount> Read()
    {
        List<GoodAmount> result = [];
        foreach (var line in lines)
        {
            Add(result, line.Id, line.Amount);
        }

        foreach (var good in hidden)
        {
            Add(result, good.GoodId, good.Amount);
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
            existing.SetAmount(existing.Amount + 1);
            return;
        }

        AddLine(id, 1);
        Notify();
    }

    void AddLine(string id, int amount)
    {
        var line = new Line(this, id, amount, goods, veInit);
        lines.Add(line);
        list.Add(line.Root);
    }

    void RemoveLine(Line line)
    {
        if (updating)
        {
            return;
        }

        lines.Remove(line);
        line.Root.RemoveFromHierarchy();
        Notify();
    }

    void Notify()
    {
        if (!updating)
        {
            Changed?.Invoke();
        }
    }

    static void Add(List<GoodAmount> result, string id, int amount)
    {
        if (amount <= 0)
        {
            return;
        }

        var index = result.FindIndex(good => good.GoodId == id);
        if (index < 0)
        {
            result.Add(new(id, amount));
            return;
        }

        result[index] = new(id, result[index].Amount + amount);
    }

    sealed class Line
    {
        readonly PostOfficeGoodEditor editor;
        readonly NineSliceIntegerField field;

        public string Id { get; }
        public int Amount { get; private set; }
        public VisualElement Root { get; }

        public Line(PostOfficeGoodEditor editor, string id, int amount, IGoodService goods, VisualElementInitializer veInit)
        {
            this.editor = editor;
            Id = id;
            Amount = Math.Clamp(amount, 1, MaxAmount);

            Root = new VisualElement().SetAsRow().AlignItems().SetMarginBottom(5);
            Root.style.flexWrap = Wrap.Wrap;
            Root.AddIconSpan().SetGood(goods, id, showName: true).SetFlexGrow().SetFlexShrink();

            field = Root.AddIntField(changeCallback: OnField)
                .SetWidth(52)
                .SetFlexShrink(0)
                .SetMargin(left: 3);
            field.isDelayed = true;
            field.SetValueWithoutNotify(Amount);
            field.Initialize(veInit);

            Root.AddPlusButton(size: UiBuilder.GameButtonSize.Small)
                .AddAction(() => SetAmount(Amount + 1))
                .SetMargin(left: 3)
                .SetFlexShrink(0);
            Root.AddMinusButton(size: UiBuilder.GameButtonSize.Small)
                .AddAction(() => SetAmount(Amount - 1))
                .SetMargin(left: 3)
                .SetFlexShrink(0);
            Root.AddGameButtonPadded("×", () => editor.RemoveLine(this), paddingY: 2)
                .SetMargin(left: 3)
                .SetFlexShrink(0);
        }

        public void SetAmount(int value)
        {
            if (editor.updating)
            {
                return;
            }

            var next = Math.Clamp(value, 1, MaxAmount);
            if (next == Amount)
            {
                if (field.value != next)
                {
                    editor.updating = true;
                    field.SetValueWithoutNotify(next);
                    editor.updating = false;
                }

                return;
            }

            Amount = next;
            editor.updating = true;
            field.SetValueWithoutNotify(next);
            editor.updating = false;
            editor.Notify();
        }

        void OnField(int value)
        {
            if (editor.updating)
            {
                return;
            }

            SetAmount(value);
        }
    }

    sealed class GoodList(IReadOnlyList<GoodSpec> specs, IGoodService goods, GoodDescriber describer) : IExtendedDropdownProvider
    {
        readonly List<string> ids = [.. specs.Select(spec => spec.Id)];
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
