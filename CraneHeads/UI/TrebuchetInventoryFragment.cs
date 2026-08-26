namespace CraneHeads.UI;

[BindFragment]
public class TrebuchetInventoryFragment(
    ILoc t,
    IGoodService goods
) : BaseEntityPanelFragment<CraneHeadTrebuchetInventory>, IEntityFragmentOrder
{
    readonly Dictionary<string, TrebuchetStockRow> payloadRows = [];
    readonly Dictionary<string, TrebuchetStockRow> costRows = [];
    Label payloadHeader = null!;
    VisualElement payloadList = null!;
    Label payloadEmpty = null!;
    Label costHeader = null!;
    VisualElement costList = null!;
    Label costEmpty = null!;

    public int Order => -40;
    public VisualElement Fragment => panel;

    protected override void InitializePanel()
    {
        payloadHeader = panel.AddGameLabel(t.T("LV.CrH.Goods")).SetMarginBottom(5);
        payloadList = panel.AddChild().SetMarginBottom(5);
        payloadEmpty = panel.AddGameLabel(t.T("LV.CrH.NoPayloadStock")).SetMarginBottom(5);
        costHeader = panel.AddGameLabel(t.T("LV.CrH.LaunchCost")).SetMarginBottom(5);
        costList = panel.AddChild();
        costEmpty = panel.AddGameLabel(t.T("LV.CrH.NoCost"));
    }

    public override void ShowFragment(BaseComponent entity)
    {
        base.ShowFragment(entity);
        if (component is null || !component.GetComponent<CraneHeadTrebuchet>().IsFinished)
        {
            panel.Visible = false;
            return;
        }

        panel.Visible = true;
        Refresh();
    }

    public override void UpdateFragment()
    {
        if (component is null || !panel.Visible)
        {
            return;
        }

        Refresh();
    }

    public override void ClearFragment()
    {
        payloadRows.Clear();
        costRows.Clear();
        payloadList.Clear();
        costList.Clear();
        base.ClearFragment();
    }

    void Refresh()
    {
        if (component is null)
        {
            return;
        }

        RefreshSection(payloadList, payloadRows, payloadEmpty, payloadHeader, component.PayloadNeed());
        RefreshCost();
    }

    void RefreshCost()
    {
        if (component is null)
        {
            return;
        }

        HashSet<string> visible = [];
        foreach (var need in component.LaunchCostNeed())
        {
            visible.Add(need.GoodId);
            if (!costRows.TryGetValue(need.GoodId, out var row))
            {
                row = new(goods, need.GoodId);
                costList.Add(row);
                costRows[need.GoodId] = row;
            }

            row.Set(component.AmountInStock(need.GoodId), need.Amount, component.LaunchCostPerShot(need.GoodId));
            row.SetDisplay(true);
        }

        foreach (var (id, row) in costRows)
        {
            if (!visible.Contains(id))
            {
                row.SetDisplay(false);
            }
        }

        var any = visible.Count > 0;
        costEmpty.SetDisplay(!any);
        costHeader.SetDisplay(true);
        costList.SetDisplay(any);
    }

    void RefreshSection(
        VisualElement list,
        Dictionary<string, TrebuchetStockRow> rows,
        Label empty,
        Label header,
        IEnumerable<GoodAmount> needs)
    {
        HashSet<string> visible = [];
        foreach (var need in needs)
        {
            visible.Add(need.GoodId);
            if (!rows.TryGetValue(need.GoodId, out var row))
            {
                row = new(goods, need.GoodId);
                list.Add(row);
                rows[need.GoodId] = row;
            }

            row.Set(component!.AmountInStock(need.GoodId), need.Amount);
            row.SetDisplay(true);
        }

        foreach (var (id, row) in rows)
        {
            if (!visible.Contains(id))
            {
                row.SetDisplay(false);
            }
        }

        var any = visible.Count > 0;
        empty.SetDisplay(!any);
        header.SetDisplay(true);
        list.SetDisplay(any);
    }
}
