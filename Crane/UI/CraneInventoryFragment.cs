namespace Crane.UI;

[BindFragment]
public class CraneInventoryFragment(
    InformationalRowsFactory informationalRowsFactory,
    ILoc t
) : BaseEntityPanelFragment<CraneInventory>
{
    readonly Dictionary<string, InformationalRow> rows = [];
    readonly HashSet<string> visibleIds = [];
    VisualElement list = null!;
    Label empty = null!;
    Inventory? shown;

    protected override void InitializePanel()
    {
        list = panel.AddChild();
        empty = panel.AddGameLabel(t.T("LV.Cr.NoMaterials"));
    }

    public override void ShowFragment(BaseComponent entity)
    {
        base.ShowFragment(entity);
        if (component is null || !component.Inventory || !component.Inventory.Enabled)
        {
            panel.Visible = false;
            return;
        }

        if (shown != component.Inventory)
        {
            ClearRows();
            shown = component.Inventory;
        }
    }

    public override void UpdateFragment()
    {
        if (component is null || !component.Inventory || !component.Inventory.Enabled)
        {
            return;
        }

        visibleIds.Clear();
        foreach (var (id, limit) in component.Limits)
        {
            if (limit > 0 || component.Inventory.AmountInStock(id) > 0)
            {
                visibleIds.Add(id);
            }
        }

        foreach (var stock in component.Inventory.Stock)
        {
            if (stock.Amount > 0)
            {
                visibleIds.Add(stock.GoodId);
            }
        }

        foreach (var id in visibleIds)
        {
            if (!rows.TryGetValue(id, out var row))
            {
                row = informationalRowsFactory.CreateInputRowWithLimit(
                    StorableGood.CreateAsGivable(id),
                    component.Inventory,
                    list);
                rows[id] = row;
            }

            row.ShowUpdated();

            var stock = component.Inventory.AmountInStock(id);
            var limit = component.Inventory.LimitedAmount(id);
            if (stock > limit)
            {
                row.Root.Q<Label>("Amount").text = stock.ToString().Color(TimberbornTextColor.Red);
            }
        }

        foreach (var (id, row) in rows)
        {
            if (!visibleIds.Contains(id))
            {
                row.Hide();
            }
        }

        empty.SetDisplay(visibleIds.Count == 0);
    }

    public override void ClearFragment()
    {
        ClearRows();
        shown = null;
        base.ClearFragment();
    }

    void ClearRows()
    {
        list.Clear();
        rows.Clear();
        visibleIds.Clear();
    }

}
