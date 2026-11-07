namespace ThePostOffice.UI;

[BindFragment]
public class PostOfficeGlobalInventoryFragment(
    ILoc t,
    IGoodService goods,
    PostOfficeGlobalInventoryService globalInventory
) : IEntityPanelFragment, IEntityFragmentOrder
{
    readonly Dictionary<string, int> incoming = [];

    EntityPanelFragmentElement panel = null!;
    Label summary = null!;
    Label empty = null!;
    VisualElement list = null!;
    PostOfficeCapacity? building;
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
        empty = panel.AddGameLabel(t.T("LV.TPO.GlobalEmpty")).SetDisplay(false);
        list = panel.AddGameScrollView();
        list.style.maxHeight = 220;
        return panel;
    }

    public void ShowFragment(BaseComponent entity)
    {
        if (!entity.TryGetComponent(out PostOfficeCapacity capacity))
        {
            ClearFragment();
            return;
        }

        building = capacity;
        showing = true;
        signature = "";
        if (capacity.IsOffice && !capacity.IsFinished)
        {
            panel.Visible = false;
            return;
        }

        panel.Visible = true;
        Refresh();
    }

    public void UpdateFragment()
    {
        var capacity = building;
        if (!showing || capacity is null || !capacity)
        {
            return;
        }

        if (capacity.IsOffice && !capacity.IsFinished)
        {
            panel.Visible = false;
            return;
        }

        if (!panel.Visible)
        {
            signature = "";
        }

        panel.Visible = true;
        Refresh();
    }

    public void ClearFragment()
    {
        panel.Visible = false;
        showing = false;
        building = null;
        signature = "";
        incoming.Clear();
        list.Clear();
    }

    void Refresh()
    {
        if (globalInventory.Inventory is null)
        {
            return;
        }

        var stored = StoredIds();
        globalInventory.CopyIncoming(incoming);
        var reserved = Sorted(incoming.Keys);
        var used = globalInventory.TotalWeight + globalInventory.IncomingWeight;
        summary.text = globalInventory.IncomingWeight > 0
            ? t.T("LV.TPO.GlobalWithReserved", used, globalInventory.Capacity, globalInventory.IncomingWeight)
            : t.T("LV.TPO.GlobalWeight", used, globalInventory.Capacity);

        var next = Sign(stored, reserved);
        if (next == signature)
        {
            return;
        }

        signature = next;
        list.Clear();
        var anyStored = stored.Count > 0;
        var anyReserved = reserved.Count > 0;
        empty.SetDisplay(!anyStored && !anyReserved);
        list.SetDisplay(anyStored || anyReserved);
        if (anyStored)
        {
            list.AddGameLabel(t.T("LV.TPO.GlobalItems").Bold()).SetMarginBottom(5);
            foreach (var id in stored)
            {
                AddGoodRow(id, globalInventory.Inventory[id].Amount);
            }
        }

        if (anyReserved)
        {
            list.AddGameLabel(t.T("LV.TPO.GlobalIncoming").Bold())
                .SetMargin(top: anyStored ? 8 : 0, bottom: 5);

            foreach (var id in reserved)
            {
                AddGoodRow(id, incoming[id]);
            }
        }
    }

    List<string> StoredIds()
    {
        List<string> ids = [];
        foreach (var pair in globalInventory.Inventory)
        {
            if (pair.Value.Amount > 0 && goods.HasGood(pair.Key))
            {
                ids.Add(pair.Key);
            }
        }

        ids.Sort(CompareName);
        return ids;
    }

    List<string> Sorted(IEnumerable<string> ids)
    {
        List<string> result = [];
        foreach (var id in ids)
        {
            if (goods.HasGood(id))
            {
                result.Add(id);
            }
        }

        result.Sort(CompareName);
        return result;
    }

    string Sign(List<string> stored, List<string> reserved)
    {
        List<string> parts =
        [
            globalInventory.TotalWeight.ToString(),
            globalInventory.IncomingWeight.ToString(),
            globalInventory.Capacity.ToString(),
        ];
        foreach (var id in stored)
        {
            parts.Add(id);
            parts.Add(globalInventory.Inventory[id].Amount.ToString());
        }

        parts.Add("#");
        foreach (var id in reserved)
        {
            parts.Add(id);
            parts.Add(incoming[id].ToString());
        }

        return string.Join("|", parts);
    }

    void AddGoodRow(string id, int amount)
    {
        var unit = globalInventory.GetItemWeight(id);
        var row = list.AddChild().SetAsRow().AlignItems().SetMarginBottom(2);
        row.AddIconSpan().SetGood(goods, id, showName: true).SetFlexGrow(0).SetFlexShrink(0);
        row.AddChild().SetMarginAuto();
        row.AddGameLabel(t.T("LV.TPO.UnitWeight", unit)).SetFlexShrink(0);
        row.AddGameLabel(amount.ToString()).SetMargin(left: 4).SetFlexShrink(0);
        row.AddGameLabel(t.T("LV.TPO.LineWeight", unit * amount)).SetMargin(left: 5).SetFlexShrink(0);
    }

    int CompareName(string left, string right)
    {
        return string.Compare(
            goods.GetGood(left).DisplayName.Value,
            goods.GetGood(right).DisplayName.Value,
            StringComparison.CurrentCultureIgnoreCase);
    }
}
