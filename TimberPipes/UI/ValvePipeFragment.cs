namespace TimberPipes.UI;

[BindFragment]
public class ValvePipeFragment(
    ILoc t,
    IGoodService goods,
    VisualElementInitializer veInit,
    DropdownItemsSetter dropdownItemsSetter
) : BaseEntityPanelFragment<ValvePipe>
{
    VisualElement inletSection = null!;    
    Toggle inletToggle = null!;
    VisualElement outletSection = null!;
    Label outletBuilding = null!;
    Toggle outletToggle = null!;
    VisualElement outletGoodRow = null!;
    Dropdown outletGood = null!;
    ValveGoodDropdownProvider outletGoods = null!;
    bool refreshing;

    protected override void InitializePanel()
    {
        inletSection = panel.AddChild().SetMarginBottom();
        inletToggle = inletSection.AddGamePanelToggle(t.T("LV.TPi.ValveInlet"), onValueChanged: OnInletChanged);

        outletSection = panel.AddChild();
        outletBuilding = outletSection.AddGameLabel().SetMarginBottom(5);

        outletGoodRow = outletSection.AddRow().AlignItems().SetMarginBottom(5);

        outletToggle = outletGoodRow.AddGamePanelToggle(t.T("LV.TPi.ValveOutlet"), OnOutletChanged)
            .SetMarginRight(5);

        outletGood = outletToggle.AddDropdown().SetFlexGrow();
        outletGood.Initialize(veInit);
        outletGoods = new(goods);
        outletGoods.Changed += OnOutletGoodChanged;
    }

    public override void ShowFragment(BaseComponent entity)
    {
        base.ShowFragment(entity);
        if (!component)
        {
            ClearFragment();
            return;
        }

        Refresh();
    }

    public override void UpdateFragment()
    {
        if (!component)
        {
            return;
        }

        Refresh();
    }

    void Refresh()
    {
        if (component is not { } valve)
        {
            return;
        }

        var inlet = valve.FindInletTarget();
        var outlet = valve.FindOutletTarget();
        if (inlet is null && outlet is null)
        {
            panel.Visible = false;
            return;
        }

        refreshing = true;
        panel.Visible = true;

        inletSection.ToggleDisplayStyle(inlet is not null);
        if (inlet is { } inletTarget)
        {
            inletToggle.text = string.Format(t.T("LV.TPi.ValveInletBuilding"), inletTarget.Building.GetLabeledName(t));
            inletToggle.SetValueWithoutNotify(valve.InletEnabled);
        }

        outletSection.ToggleDisplayStyle(outlet is not null);
        if (outlet is { } outletTarget)
        {
            outletBuilding.text = string.Format(t.T("LV.TPi.ValveOutletBuilding"), outletTarget.Building.GetLabeledName(t));
            outletToggle.SetValueWithoutNotify(valve.OutletEnabled);
            RefreshOutletGoods(valve);
        }

        refreshing = false;
    }

    void OnInletChanged(bool enabled)
    {
        if (refreshing || component is not { } valve)
        {
            return;
        }

        valve.InletEnabled = enabled;
    }

    void OnOutletChanged(bool enabled)
    {
        if (refreshing || component is not { } valve)
        {
            return;
        }

        valve.OutletEnabled = enabled;
        if (enabled && valve.OutletGoodId is null)
        {
            refreshing = true;
            RefreshOutletGoods(valve);
            refreshing = false;
        }
    }

    void OnOutletGoodChanged(string goodId)
    {
        if (refreshing || component is not { } valve)
        {
            return;
        }

        valve.OutletGoodId = goodId is { Length: > 0 } ? goodId : null;
    }

    void RefreshOutletGoods(ValvePipe valve)
    {
        var ids = OutletGoodIds(valve);
        if (ids.Count == 0)
        {
            valve.OutletGoodId = null;
            BindOutletGoods([], "");
            return;
        }

        var selected = valve.OutletGoodId is { } id && ids.Contains(id) ? id : ids[0];
        valve.OutletGoodId = selected;
        BindOutletGoods(ids, selected);
    }

    void BindOutletGoods(List<string> ids, string selectedId)
    {
        outletGoodRow.ToggleDisplayStyle(ids.Count > 0);
        if (ids.Count == 0)
        {
            if (outletGoods.Ids.Count > 0)
            {
                outletGoods.Ids.Clear();
                outletGoods.SelectedId = "";
                outletGood.ClearItems();
            }

            return;
        }

        var itemsChanged = !outletGoods.Ids.SequenceEqual(ids);
        outletGoods.SelectedId = selectedId;
        if (itemsChanged)
        {
            outletGoods.Ids.Clear();
            outletGoods.Ids.AddRange(ids);
            dropdownItemsSetter.SetItems(outletGood, outletGoods);
        }
        else
        {
            outletGood.UpdateSelectedValue();
        }
    }

    List<string> OutletGoodIds(ValvePipe valve)
    {
        List<string> ids = [.. valve.OutletGoodIds()];
        ids.Sort((a, b) => goods.GetGood(a).GoodOrder.CompareTo(goods.GetGood(b).GoodOrder));
        return ids;
    }
}

class ValveGoodDropdownProvider(IGoodService goods) : IExtendedDropdownProvider
{
    public List<string> Ids { get; } = [];
    public string SelectedId { get; set; } = "";
    public event Action<string>? Changed;

    public IReadOnlyList<string> Items => Ids;

    public string GetValue() => SelectedId;

    public void SetValue(string value)
    {
        if (SelectedId == value)
        {
            return;
        }

        SelectedId = value;
        Changed?.Invoke(value);
    }

    public string FormatDisplayText(string value, bool selected)
    {
        if (goods.HasGood(value))
        {
            return goods.GetGood(value).DisplayName.Value;
        }

        return value;
    }

    public Sprite GetIcon(string value)
    {
        if (goods.HasGood(value))
        {
            return goods.GetGood(value).Icon.Asset;
        }

        return null!;
    }

    public ImmutableArray<string> GetItemClasses(string value) => [];
}
