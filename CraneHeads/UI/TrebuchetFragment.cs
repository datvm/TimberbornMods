namespace CraneHeads.UI;

[BindFragment]
public class TrebuchetFragment(
    ILoc t,
    IGoodService goods,
    VisualElementInitializer veInit,
    DropdownItemsSetter dropdownItems,
    TrebuchetTargetTool targetTool
) : BaseEntityPanelFragment<CraneHeadTrebuchet>
{
    DropdownRow<string?> good = null!;
    NineSliceIntegerField amount = null!;
    Label weight = null!;
    Button target = null!;
    Toggle repeatable = null!;
    Label status = null!;
    bool updating;

    protected override void InitializePanel()
    {
        good = panel.AddDropdownRow<string?>(
            t.T("LV.CrH.Payload"),
            OnGoodChanged,
            veInit,
            dropdownItems);
        good.SetMarginBottom(5);

        var amountRow = panel.AddRow().AlignItems().SetMarginBottom(5);
        amountRow.AddGameLabel(t.T("LV.CrH.Amount")).SetMarginRight(5).SetFlexShrink(0);
        amount = amountRow.AddIntField(changeCallback: OnAmountChanged).SetFlexGrow().Initialize(veInit);

        weight = panel.AddGameLabel().SetMarginBottom(5);
        target = panel.AddGameButtonPadded(t.T("LV.CrH.ChooseTarget"), ChooseTarget).SetFlexGrow().SetMarginBottom(5);
        repeatable = panel.AddToggle(t.T("LV.CrH.Repeatable"), onValueChanged: OnRepeatableChanged);
        status = panel.AddGameLabel();

        panel.Initialize(veInit);
        good.SetItems(GoodItems());
    }

    public override void ShowFragment(BaseComponent entity)
    {
        base.ShowFragment(entity);
        if (Shown is null)
        {
            return;
        }

        Refresh();
    }

    public override void UpdateFragment()
    {
        base.UpdateFragment();
        if (Shown is null)
        {
            return;
        }

        RefreshStatus();
        RefreshTargetButton();
    }

    void Refresh()
    {
        var c = Shown;
        if (c is null)
        {
            return;
        }

        updating = true;
        good.SetSelectedValueWithoutNotifying(c.GoodId);
        amount.SetValueWithoutNotify(c.Amount);
        repeatable.SetValueWithoutNotify(c.Repeatable);
        updating = false;
        RefreshWeight();
        RefreshTargetButton();
        RefreshStatus();
    }

    void RefreshWeight()
    {
        var c = Shown;
        if (c is null)
        {
            return;
        }

        var used = c.GoodId is null ? 0 : c.WeightOf(c.Amount);
        weight.text = t.T("LV.CrH.Weight", used, c.Spec.WeightLimit, c.CostDescription());
    }

    void RefreshTargetButton()
    {
        var c = Shown;
        if (c is null)
        {
            return;
        }

        target.text = c.Target is { } dest
            ? t.T("LV.CrH.ChangeTarget", dest.x, dest.y, dest.z)
            : t.T("LV.CrH.ChooseTarget");
    }

    void RefreshStatus()
    {
        var c = Shown;
        if (c is null)
        {
            return;
        }

        if (!c.HasOrder)
        {
            status.text = t.T("LV.CrH.TrebuchetNeedOrder", c.MaxRange);
            return;
        }

        if (!c.InRange(c.Target!.Value))
        {
            status.text = t.T("LV.CrH.TrebuchetOutOfRange", c.MaxRange);
            return;
        }

        if (!c.IsPathClear())
        {
            status.text = t.T("LV.CrH.TrebuchetBlocked");
            return;
        }

        status.text = t.T("LV.CrH.TrebuchetReady", c.MaxRange);
    }

    void OnGoodChanged(IndexedDropdownRowItem<string?> item)
    {
        var c = Shown;
        if (updating || c is null)
        {
            return;
        }

        var amountValue = c.Amount <= 0 ? 1 : c.Amount;
        c.SetPayload(item.Item.Value, amountValue);
        amount.SetValueWithoutNotify(c.Amount);
        RefreshWeight();
        RefreshStatus();
    }

    void OnAmountChanged(int value)
    {
        var c = Shown;
        if (updating || c is null || c.GoodId is null)
        {
            return;
        }

        c.SetPayload(c.GoodId, value);
        if (amount.value != c.Amount)
        {
            amount.SetValueWithoutNotify(c.Amount);
        }

        RefreshWeight();
    }

    void OnRepeatableChanged(bool value)
    {
        var c = Shown;
        if (updating || c is null)
        {
            return;
        }

        c.SetRepeatable(value);
    }

    void ChooseTarget()
    {
        var c = Shown;
        if (c is not null)
        {
            targetTool.Begin(c);
        }
    }

    CraneHeadTrebuchet? Shown => component is { } c && c ? c : null;

    IEnumerable<DropdownRowItem<string?>> GoodItems()
    {
        yield return new(null, t.T("LV.CrH.NoGood"));
        foreach (var id in goods.Goods.OrderBy(id => goods.GetGood(id).DisplayName.Value))
        {
            yield return new(id, goods.GetGood(id).DisplayName.Value);
        }
    }
}
