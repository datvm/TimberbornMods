namespace GlobalInventory.UI;

[BindTransient]
public class ModifyGlobalInventoryDialog(
    PanelStack panelStack,
    VisualElementInitializer veInit,
    GlobalInventoryService inventory,
    ILoc t
) : DialogBoxElement
{
    VisualElement list = null!;
    Label empty = null!;
    VisualElement details = null!;
    Label itemName = null!;
    Label current = null!;
    FloatField amount = null!;
    Button fill = null!;
    Button setMin = null!;
    List<GoodButton> rows = [];
    ToggleGroup<SortMode> sort = null!;
    GlobalGoodHandle? selected;
    string query = "";
    bool nonZeroOnly;
    bool built;

    public void Show()
    {
        if (!built)
        {
            Build();
            built = true;
        }

        Refresh();
        Show(veInit, panelStack);
    }

    void Build()
    {
        SetTitle(t.T("LV.GI.Dev.Title"));
        AddCloseButton();
        SetDialogSize(width: 760, height: 540);

        var body = Content.AddChild().SetAsRow().SetFlexGrow();
        var left = body.AddChild().SetWidth(240).SetMarginRight(15).SetFlexShrink(0);

        var search = left.AddTextField(changeCallback: OnQuery).SetMarginBottom(5).SetFlexShrink(0);
        search.textEdition.placeholder = t.T("LV.GI.Dev.Filter");
        search.textEdition.hidePlaceholderOnFocus = false;

        left.AddToggle(t.T("LV.GI.Dev.NonZero"), onValueChanged: OnNonZero).SetMarginBottom(5).SetFlexShrink(0);
        left.AddGameLabel(t.T("LV.GI.Dev.Sort")).SetMargin(top: 5, bottom: 5).SetFlexShrink(0);
        sort = new ToggleGroup<SortMode>(
            [SortMode.Alphabetical, SortMode.Amount, SortMode.Order],
            mode => left.AddToggle(SortLabel(mode)).SetMarginBottom(5).SetFlexShrink(0));
        sort.SetValueWithoutNotify(SortMode.Order);
        sort.OnValueChanged += (_, _) => Refresh();

        empty = left.AddGameLabel(t.T("LV.GI.Nothing")).SetFlexShrink(0);
        list = left.AddScrollView().SetFlexGrow().SetFlexShrink();
        list.style.minHeight = 0f;

        details = body.AddChild().SetFlexGrow();
        itemName = details.AddGameLabel().SetMarginBottom(5);
        current = details.AddGameLabel().SetMarginBottom();

        details.AddGameLabel(t.T("LV.GI.Dev.Amount")).SetMarginBottom(5);
        amount = details.AddFloatField().SetMarginBottom();
        amount.SetValueWithoutNotify(1);

        var adjust = details.AddChild().SetMarginBottom(5);
        adjust.AddMenuButton(t.T("LV.GI.Dev.Add"), () => Adjust(false), stretched: true).SetMarginBottom(5);
        adjust.AddMenuButton(t.T("LV.GI.Dev.Remove"), () => Adjust(true), stretched: true).SetMarginBottom(5);
        adjust.AddMenuButton(t.T("LV.GI.Dev.Clear"), ClearAmount, stretched: true).SetMarginBottom(5);
        fill = adjust.AddMenuButton(t.T("LV.GI.Dev.Fill"), Fill, stretched: true).SetMarginBottom(5);
        setMin = adjust.AddMenuButton(t.T("LV.GI.Dev.SetMin"), SetToMin, stretched: true);
    }

    void Select(GlobalGoodHandle handle)
    {
        selected = handle;
        Refresh();
    }

    void Adjust(bool removeAmount)
    {
        if (selected is not { } handle || amount.value <= 0)
        {
            return;
        }

        if (removeAmount)
        {
            handle.RemoveSafe(amount.value);
        }
        else
        {
            handle.AddSafe(amount.value);
        }

        Refresh();
    }

    void ClearAmount()
    {
        if (selected is not { } handle)
        {
            return;
        }

        handle.Set(handle.MinCapacity > 0 ? handle.MinCapacity : 0);
        Refresh();
    }

    void Fill()
    {
        if (selected is not { } handle || handle.MaxCapacity is not float max)
        {
            return;
        }

        handle.Set(max);
        Refresh();
    }

    void SetToMin()
    {
        if (selected is not { } handle || handle.MinCapacity >= 0)
        {
            return;
        }

        handle.Set(handle.MinCapacity);
        Refresh();
    }

    void Refresh()
    {
        EnsureRows();
        foreach (var row in rows)
        {
            var text = $"{row.Handle.Spec.DisplayName.Value} ({GlobalInventoryUi.FormatAmount(row.Handle)})";
            row.Button.text = row.Handle == selected ? text.Bold() : text;
        }

        var hasSelection = selected is not null;
        empty.SetDisplay(rows.Count == 0);
        details.SetDisplay(hasSelection);
        if (selected is not { } handle)
        {
            return;
        }

        itemName.text = handle.Spec.DisplayName.Value.Bold();
        current.text = t.T("LV.GI.Dev.Current", GlobalInventoryUi.FormatAmount(handle));
        fill.SetDisplay(handle.MaxCapacity is not null);
        setMin.SetDisplay(handle.MinCapacity < 0);
    }

    void OnQuery(string value)
    {
        query = value;
        Refresh();
    }

    void OnNonZero(bool value)
    {
        nonZeroOnly = value;
        Refresh();
    }

    string SortLabel(SortMode mode) => mode switch
    {
        SortMode.Alphabetical => t.T("LV.GI.Dev.SortAlpha"),
        SortMode.Amount => t.T("LV.GI.Dev.SortAmount"),
        _ => t.T("LV.GI.Dev.SortOrder"),
    };

    void EnsureRows()
    {
        var visible = VisibleGoods();
        if (SameRows(visible))
        {
            return;
        }

        list.Clear();
        rows = [];
        foreach (var handle in visible)
        {
            var captured = handle;
            var button = list.AddGameButtonPadded("", () => Select(captured), stretched: true).SetMarginBottom(5);
            rows.Add(new(handle, button));
        }
    }

    bool SameRows(List<GlobalGoodHandle> visible)
    {
        if (rows.Count != visible.Count)
        {
            return false;
        }

        for (var i = 0; i < rows.Count; i++)
        {
            if (rows[i].Handle != visible[i])
            {
                return false;
            }
        }

        return true;
    }

    List<GlobalGoodHandle> VisibleGoods()
    {
        IEnumerable<GlobalGoodHandle> items = inventory.All;
        if (nonZeroOnly)
        {
            items = items.Where(q => q.Amount != 0);
        }

        var needle = query.Trim();
        if (needle.Length > 0)
        {
            items = items.Where(q => q.Spec.DisplayName.Value.Contains(needle, StringComparison.CurrentCultureIgnoreCase));
        }

        items = sort.Value switch
        {
            SortMode.Alphabetical => items
                .OrderBy(q => q.Spec.DisplayName.Value, StringComparer.CurrentCultureIgnoreCase)
                .ThenBy(q => q.Spec.Order),
            SortMode.Amount => items
                .OrderByDescending(q => q.Amount)
                .ThenBy(q => q.Spec.Order)
                .ThenBy(q => q.Spec.DisplayName.Value, StringComparer.CurrentCultureIgnoreCase),
            _ => items,
        };

        return [.. items];
    }

    enum SortMode
    {
        Alphabetical,
        Amount,
        Order,
    }

    readonly record struct GoodButton(GlobalGoodHandle Handle, Button Button);
}
