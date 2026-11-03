namespace ThePostOffice.UI;

[BindTransient]
public class PostOfficeOrderDialog(
    IContainer container,
    PanelStack panelStack,
    VisualElementInitializer veInit,
    DropdownItemsSetter dropdowns,
    IGoodService goods,
    GoodDescriber describer,
    ILoc t,
    PostOfficeService offices
) : DialogBoxElement
{
    readonly List<GoodAmount> result = [];
    readonly List<OrderItemElement> items = [];
    
    PostOfficeAmountEditor.Catalog catalog = null!;
    VisualElement list = null!;
    Label totalWeight = null!;
    Label parcelCount = null!;
    Label parcelLimit = null!;
    Button confirm = null!;

    void Initialize()
    {
        var parent = Content;
        SetTitle(t.T("LV.TPO.ManualOrder"));
        AddCloseButton();
        SetDialogSize(width: 460, height: 480);

        parent.AddGameLabel(t.T("LV.TPO.Good")).SetMarginBottom(5);
        var addRow = parent.AddRow().AlignItems().SetMarginBottom(5);
        var specs = Mailable();
        catalog = new(specs, goods, describer);
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

        totalWeight = parent.AddGameLabel().SetMarginBottom(2);
        parcelCount = parent.AddGameLabel().SetMarginBottom(2);
        parcelLimit = parent.AddGameLabel(t.T("LV.TPO.ParcelLimit", offices.ParcelWeight)).SetMarginBottom(5);
        confirm = parent.AddGameButtonPadded(t.T("LV.TPO.Confirm"), Confirm, paddingY: 3).SetMarginBottom(8);

        parent.AddGameLabel(t.T("LV.TPO.AddedGoods"), bold: true).SetMarginBottom(5);
        list = parent.AddChild();
        Refresh();
    }

    public async Task<IReadOnlyList<GoodAmount>?> ShowAsync()
    {
        Initialize();
        return await ShowAsync(veInit, panelStack) ? result : null;
    }

    void AddSelected()
    {
        if (catalog.GetValue() is not { Length: > 0 } id)
        {
            return;
        }

        var existing = items.Find(item => item.Id == id);
        if (existing is not null)
        {
            existing.SetAmount(existing.Amount + 1);
            return;
        }

        var item = container.GetInstance<OrderItemElement>();
        item.SetGood(id, 1);
        item.Changed += Refresh;
        item.Removed += () => Remove(item);
        items.Add(item);
        list.Add(item);
        Refresh();
    }

    void Remove(OrderItemElement item)
    {
        items.Remove(item);
        item.RemoveFromHierarchy();
        Refresh();
    }

    void Confirm()
    {
        var next = Read();
        if (offices.Split(next).Count == 0)
        {
            return;
        }

        result.Clear();
        result.AddRange(next);
        OnUIConfirmed();
    }

    void Refresh()
    {
        var next = Read();
        var count = offices.Split(next).Count;
        totalWeight.text = t.T("LV.TPO.TotalWeight", offices.GoodsWeight(next));
        parcelCount.text = t.T("LV.TPO.ParcelsToAdd", count);
        confirm.SetEnabled(count > 0);
    }

    List<GoodAmount> Read()
    {
        List<GoodAmount> next = [];
        foreach (var item in items)
        {
            if (item.Amount > 0)
            {
                next.Add(new(item.Id, item.Amount));
            }
        }

        return next;
    }

    List<GoodSpec> Mailable()
    {
        List<GoodSpec> catalogSpecs = [];
        foreach (var id in goods.Goods)
        {
            if (!goods.HasGood(id))
            {
                continue;
            }

            var spec = goods.GetGood(id);
            if (spec.GoodType == "Liquid" || spec.Weight <= 0)
            {
                continue;
            }

            catalogSpecs.Add(spec);
        }

        catalogSpecs.Sort((left, right) => string.Compare(
            left.DisplayName.Value,
            right.DisplayName.Value,
            StringComparison.CurrentCultureIgnoreCase));
        return catalogSpecs;
    }

}
