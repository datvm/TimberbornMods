namespace CraneHeads.UI;

[BindFragment]
public class TrebuchetFragment(
    ILoc t,
    IGoodService goods,
    IContainer container,
    VisualElementInitializer veInit,
    DropdownItemsSetter dropdownItems,
    TrebuchetTargetTool targetTool
) : BaseEntityPanelFragment<CraneHeadTrebuchet>, IEntityFragmentOrder
{
    Label range = null!;
    Button fire = null!;
    Toggle repeat = null!;
    VisualElement payloadList = null!;
    DropdownRow<string?> addGood = null!;
    Label weight = null!;
    Button target = null!;
    CraneHeadTrebuchetInventory? inventory;
    CraneHeadTrebuchetLauncher? launcher;
    string? pendingAddGood;
    bool updating;

    public int Order => -50;
    public VisualElement Fragment => panel;

    protected override void InitializePanel()
    {
        range = panel.AddGameLabel().SetMarginBottom(5);
        target = panel.AddGameButtonPadded(t.T("LV.CrH.ChooseTarget"), ChooseTarget).SetFlexGrow().SetMarginBottom();

        panel.AddGameLabel(t.T("LV.CrH.Payload")).SetMarginBottom(5);
        payloadList = panel.AddChild().SetMarginBottom();

        var addRow = panel.AddRow().AlignItems().SetMarginBottom();
        addGood = addRow.AddDropdownRow<string?>(null, OnAddGoodChanged, veInit, dropdownItems).SetFlexGrow();
        addRow.AddGameButtonPadded(t.T("LV.CrH.AddGood"), AddSelectedGood).SetMargin(left: 5);

        weight = panel.AddGameLabel().SetMarginBottom(5);

        var fireRow = panel.AddRow().AlignItems().SetMarginBottom();
        fire = fireRow.AddGameButtonPadded(t.T("LV.CrH.Fire"), Fire).SetFlexGrow();
        repeat = fireRow.AddGamePanelToggle(t.T("LV.CrH.LaunchRepeat"), OnRepeatChanged)
            .SetMargin(left: 5)
            .SetFlexGrow(0)
            .SetFlexShrink(0);        

        panel.Initialize(veInit);
    }

    public override void ShowFragment(BaseComponent entity)
    {
        base.ShowFragment(entity);
        DetachInventory();
        if (Shown is not { } c || !c.IsFinished)
        {
            panel.Visible = false;
            return;
        }

        inventory = c.GetComponent<CraneHeadTrebuchetInventory>();
        launcher = c.GetComponent<CraneHeadTrebuchetLauncher>();
        if (inventory)
        {
            inventory.Changed += OnInventoryChanged;
        }

        panel.Visible = true;
        Refresh();
        launcher?.OnShown();
    }

    public override void UpdateFragment()
    {
        base.UpdateFragment();
        if (Shown is null || !Shown.IsFinished)
        {
            return;
        }

        RefreshRange();
        RefreshTargetButton();
        RefreshFire();
    }

    public override void ClearFragment()
    {
        DetachInventory();
        launcher = null;
        base.ClearFragment();
    }

    void Refresh()
    {
        var c = Shown;
        if (c is null || inventory is null)
        {
            return;
        }

        updating = true;
        repeat.SetValueWithoutNotify(c.Mode == TrebuchetLaunchMode.Repeat);
        RefreshPayloadRows();
        RefreshAddGoods();
        updating = false;
        RefreshRange();
        RefreshWeight();
        RefreshTargetButton();
        RefreshFire();
    }

    void RefreshPayloadRows()
    {
        payloadList.Clear();
        if (inventory is null)
        {
            return;
        }

        foreach (var (id, amount) in inventory.Requested)
        {
            payloadList.AddChild(container.GetInstance<TrebuchetPayloadRow>)
                .Init(Refresh, RefreshWeight)
                .Bind(inventory, id, amount);
        }
    }

    void RefreshAddGoods()
    {
        List<DropdownRowItem<string?>> items = [new(null, t.T("LV.CrH.NoGood"))];
        if (inventory is not null)
        {
            foreach (var id in goods.Goods.OrderBy(id => goods.GetGood(id).DisplayName.Value))
            {
                if (inventory.Requested.ContainsKey(id))
                {
                    continue;
                }

                items.Add(new(id, goods.GetGood(id).DisplayName.Value));
            }
        }

        addGood.SetItems(items);
        addGood.SetSelectedValueWithoutNotifying(null);
        pendingAddGood = null;
    }

    void RefreshRange()
    {
        if (Shown is not { } c)
        {
            return;
        }

        range.text = t.T("LV.CrH.TrebuchetRange", c.MaxRange);
    }

    void RefreshWeight()
    {
        var c = Shown;
        if (c is null || inventory is null)
        {
            return;
        }

        weight.text = t.T("LV.CrH.Weight", inventory.PayloadWeight, inventory.WeightLimit);
        weight.style.color = inventory.IsOverweight
            ? TimberUiUtils.DangerColor
            : new StyleColor(StyleKeyword.Null);
    }

    void RefreshTargetButton()
    {
        target.text = launcher?.Target is { } dest
            ? t.T("LV.CrH.ChangeTarget", dest.x, dest.y, dest.z)
            : t.T("LV.CrH.ChooseTarget");
    }

    void RefreshFire()
    {
        fire.SetEnabled(launcher is { } l && l.CanFire);
    }

    void Fire()
    {
        launcher?.Fire();
    }

    void OnRepeatChanged(bool on)
    {
        if (updating || launcher is null)
        {
            return;
        }

        launcher.SetRepeat(on);
        RefreshFire();
    }

    void OnAddGoodChanged(IndexedDropdownRowItem<string?> item)
        => pendingAddGood = item.Item.Value;

    void AddSelectedGood()
    {
        if (pendingAddGood is not { } id || inventory is null)
        {
            return;
        }

        inventory.TrySetGood(id, 1);
        Refresh();
    }

    void OnInventoryChanged(object sender, EventArgs e)
    {
        if (updating)
        {
            return;
        }

        RefreshWeight();
    }

    void ChooseTarget()
    {
        if (launcher is not null)
        {
            targetTool.Begin(launcher);
        }
    }

    void DetachInventory()
    {
        if (inventory is not null)
        {
            inventory.Changed -= OnInventoryChanged;
        }

        inventory = null;
    }

    CraneHeadTrebuchet? Shown => component is { } c && c ? c : null;
}
