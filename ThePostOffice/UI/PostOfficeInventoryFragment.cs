namespace ThePostOffice.UI;

[BindFragment]
public class PostOfficeInventoryFragment(
    InformationalRowsFactory rows,
    IGoodService goods,
    ILoc t
) : BaseEntityPanelFragment<PostOfficeInventory>, IEntityFragmentOrder
{
    readonly Dictionary<string, InformationalRow> shown = [];
    VisualElement list = null!;
    Label empty = null!;
    string signature = "";

    public int Order => 10;
    public VisualElement Fragment => panel;

    protected override void InitializePanel()
    {
        list = panel.AddChild();
        empty = panel.AddGameLabel(t.T("LV.TPO.EmptyStorage"));
    }

    public override void ShowFragment(BaseComponent entity)
    {
        base.ShowFragment(entity);
        signature = "";
        if (!Ready())
        {
            panel.Visible = false;
            return;
        }

        panel.Visible = true;
        Refresh();
    }

    public override void UpdateFragment()
    {
        if (!Ready())
        {
            panel.Visible = false;
            return;
        }

        panel.Visible = true;
        Refresh();
    }

    public override void ClearFragment()
    {
        list.Clear();
        shown.Clear();
        signature = "";
        base.ClearFragment();
    }

    void Refresh()
    {
        var inventory = component!.Inventory;
        List<string> stock = [];
        foreach (var good in inventory.Stock)
        {
            if (good.Amount > 0 && goods.HasGood(good.GoodId))
            {
                stock.Add(good.GoodId);
            }
        }

        stock.Sort((left, right) => string.Compare(
            goods.GetGood(left).DisplayName.Value,
            goods.GetGood(right).DisplayName.Value,
            StringComparison.CurrentCultureIgnoreCase));

        var next = string.Join("|", stock);
        if (next != signature)
        {
            list.Clear();
            shown.Clear();
            signature = next;
            foreach (var id in stock)
            {
                shown[id] = rows.CreateSimpleRowWithoutLimit(
                    StorableGood.CreateAsGivable(id),
                    inventory,
                    list);
            }
        }

        foreach (var row in shown.Values)
        {
            row.ShowUpdated();
        }

        empty.SetDisplay(stock.Count == 0);
    }

    bool Ready()
    {
        var inventory = component;
        return inventory is not null && inventory && inventory.Inventory && inventory.Inventory.Enabled;
    }
}
