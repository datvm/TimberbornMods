namespace ThePostOffice.UI;

[BindFragment]
public class PostOfficeGlobalInventoryFragment(
    ILoc t,
    IGoodService goods,
    PostOfficeGlobalInventoryService globalInventory
) : IEntityPanelFragment, IEntityFragmentOrder
{
    readonly List<GlobalRow> rows = [];

    EntityPanelFragmentElement panel = null!;
    Label summary = null!;
    Label contribution = null!;
    Label empty = null!;
    VisualElement list = null!;
    PostOfficeCapacitySpec? capacity;
    BlockObject? block;
    DistrictBuilding? district;
    bool showing;
    string signature = "";

    public int Order => 200;
    public VisualElement Fragment => panel;

    public VisualElement InitializeFragment()
    {
        panel = new EntityPanelFragmentElement
        {
            Visible = false,
            Background = EntityPanelFragmentBackground.Blue,
        };

        summary = panel.AddGameLabel().SetMarginBottom(5);
        contribution = panel.AddGameLabel().SetMarginBottom(5);
        empty = panel.AddGameLabel(t.T("LV.TPO.GlobalEmpty"));
        list = panel.AddGameScrollView();
        list.style.maxHeight = 220;
        return panel;
    }

    public void ShowFragment(BaseComponent entity)
    {
        if (!entity.HasComponent<PostOfficeCapacitySpec>())
        {
            ClearFragment();
            return;
        }

        capacity = entity.GetComponent<PostOfficeCapacitySpec>();
        block = entity.GetComponent<BlockObject>();
        district = entity.GetComponent<DistrictBuilding>();
        showing = true;
        panel.Visible = true;
        signature = "";
        Refresh();
    }

    public void UpdateFragment()
    {
        if (!showing)
        {
            return;
        }

        Refresh();
    }

    public void ClearFragment()
    {
        panel.Visible = false;
        showing = false;
        capacity = null;
        block = null;
        district = null;
        signature = "";
        foreach (var row in rows)
        {
            row.Hide();
        }
    }

    void Refresh()
    {
        if (globalInventory.Inventory is null)
        {
            return;
        }

        var shown = Collect();
        var next = Sign(shown);
        summary.text = t.T(
            "LV.TPO.GlobalWeight",
            globalInventory.TotalWeight,
            globalInventory.ReservedWeight,
            globalInventory.Capacity);
        var building = capacity;
        if (building is null)
        {
            contribution.SetDisplay(false);
        }
        else
        {
            contribution.SetDisplay(true);
            contribution.text = Connected()
                ? t.T("LV.TPO.Contribution", building.Contribution)
                : t.T("LV.TPO.ContributionPending", building.Contribution);
        }

        if (next == signature)
        {
            return;
        }

        signature = next;
        empty.SetDisplay(shown.Count == 0);
        list.SetDisplay(shown.Count > 0);
        EnsureRows(shown.Count);
        for (var i = 0; i < shown.Count; i++)
        {
            rows[i].Bind(goods, t, shown[i]);
        }

        for (var i = shown.Count; i < rows.Count; i++)
        {
            rows[i].Hide();
        }
    }

    List<ShownGood> Collect()
    {
        List<ShownGood> shown = [];
        foreach (var pair in globalInventory.Inventory)
        {
            var reserved = globalInventory.Reserved.TryGetValue(pair.Key, out var hold) ? hold.Amount : 0;
            if (pair.Value.Amount <= 0 && reserved <= 0)
            {
                continue;
            }

            shown.Add(new(pair.Value, reserved));
        }

        shown.Sort(Compare);
        return shown;
    }

    string Sign(List<ShownGood> shown)
    {
        var building = capacity;
        List<string> parts =
        [
            globalInventory.TotalWeight.ToString(),
            globalInventory.ReservedWeight.ToString(),
            globalInventory.Capacity.ToString(),
            Connected() ? "1" : "0",
            building is null ? "" : building.Contribution.ToString(),
        ];
        foreach (var good in shown)
        {
            parts.Add(good.Good.GoodSpec.Id);
            parts.Add(good.Good.Amount.ToString());
            parts.Add(good.Reserved.ToString());
        }

        return string.Join("|", parts);
    }

    void EnsureRows(int count)
    {
        while (rows.Count < count)
        {
            rows.Add(new(list));
        }
    }

    bool Connected()
    {
        var body = block;
        var link = district;
        return body is not null && body && body.IsFinished
            && link is not null && link && link.InstantDistrict;
    }

    static int Compare(ShownGood left, ShownGood right)
    {
        return string.Compare(
            left.Good.GoodSpec.DisplayName.Value,
            right.Good.GoodSpec.DisplayName.Value,
            StringComparison.CurrentCultureIgnoreCase);
    }

    readonly record struct ShownGood(PostOfficeInventoryGood Good, int Reserved);

    sealed class GlobalRow
    {
        readonly VisualElement root;
        readonly IconSpan icon;
        readonly Label detail;
        string id = "";
        int amount = -1;

        public GlobalRow(VisualElement parent)
        {
            root = parent.AddRow().AlignItems().SetMarginBottom(2);
            icon = root.AddIconSpan().SetFlexGrow().SetFlexShrink();
            detail = root.AddGameLabel().SetFlexShrink(0);
        }

        public void Bind(IGoodService goods, ILoc t, ShownGood shown)
        {
            root.SetDisplay(true);
            var good = shown.Good;
            var nextId = good.GoodSpec.Id;
            if (id != nextId || amount != good.Amount)
            {
                id = nextId;
                amount = good.Amount;
                icon.SetGood(goods, nextId, good.Amount.ToString(), showName: true);
            }

            detail.text = shown.Reserved > 0
                ? t.T("LV.TPO.Reserved", shown.Reserved, shown.Reserved * good.ItemWeight)
                : t.T("LV.TPO.StoredWeight", good.TotalWeight);
        }

        public void Hide()
        {
            root.SetDisplay(false);
            id = "";
            amount = -1;
        }
    }
}
