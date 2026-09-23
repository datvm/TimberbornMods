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
    GlobalGoodHandle? selected;
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
        SetDialogSize(width: 760, height: 420);

        var body = Content.AddChild().SetAsRow().SetFlexGrow();
        var left = body.AddChild().SetWidth(200).SetMarginRight(15);
        empty = left.AddGameLabel(t.T("LV.GI.Nothing"));
        list = left.AddScrollView();

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

    void EnsureRows()
    {
        if (rows.Count == inventory.All.Count)
        {
            return;
        }

        list.Clear();
        rows = [];
        foreach (var handle in inventory.All)
        {
            var captured = handle;
            var button = list.AddGameButtonPadded("", () => Select(captured), stretched: true).SetMarginBottom(5);
            rows.Add(new(handle, button));
        }
    }

    readonly record struct GoodButton(GlobalGoodHandle Handle, Button Button);
}
