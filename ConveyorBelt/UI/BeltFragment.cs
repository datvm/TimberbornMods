namespace ConveyorBelt.UI;

[BindFragment]
public class BeltFragment(ILoc t, IGoodService goods, NamedIconProvider icons) : BaseEntityPanelFragment<BeltInventory>
{
    public const int IconSize = 30;
    public const int Padding = 5;

    readonly List<BeltItemElement> slots = [];

    Toggle warn = null!;
    NineSliceButton eject = null!;
    VisualElement goodsBar = null!;
    bool refreshing;

    protected override void InitializePanel()
    {
        goodsBar = panel.AddChild().SetMarginBottom(10);
        var bar = goodsBar.style;
        bar.height = IconSize + Padding * 2;
        bar.backgroundColor = TimberUiUtils.WarningColor;

        eject = panel.AddGameButtonPadded(t.T("LV.CBlt.Eject"), OnEject, stretched: true).SetMarginBottom(10);
        warn = panel.AddGamePanelToggle(t.T("LV.CBlt.WarnStuck"), OnWarn);
    }

    public override void ShowFragment(BaseComponent entity)
    {
        base.ShowFragment(entity);
        Refresh();
    }

    public override void UpdateFragment() => Refresh();

    void Refresh()
    {
        if (component is not BeltInventory inventory)
        {
            return;
        }

        refreshing = true;
        warn.SetValueWithoutNotify(inventory.WarnWhenStuck);
        refreshing = false;

        var items = inventory.Items;
        eject.SetEnabled(items.Count > 0);
        EnsureSlots(items.Count);
        var width = goodsBar.resolvedStyle.width - IconSize - Padding * 2;
        for (var i = 0; i < items.Count; i++)
        {
            var item = items[i];
            var slot = slots[i];
            slot.SetDisplay(true);
            slot.Icon = goods.GetGood(item.Id).Icon.Asset;
            slot.Progress = Padding + Mathf.Lerp(0f, width, item.Position);
            slot.IsStuck = inventory.IsStuck && i == 0;
        }
    }

    void EnsureSlots(int count)
    {
        if (slots.Count < count)
        {
            var stuckIcon = icons.GetOrLoad("error-icon", "UI/Images/Core/error-icon");
            for (var i = slots.Count; i < count; i++)
            {
                var slot = new BeltItemElement(stuckIcon);
                goodsBar.Add(slot);
                slots.Add(slot);
            }
        }

        for (var i = count; i < slots.Count; i++)
        {
            slots[i].SetDisplay(false);
        }
    }

    void OnEject()
    {
        if (component is not BeltInventory inventory)
        {
            return;
        }

        inventory.Eject();
    }

    void OnWarn(bool value)
    {
        if (refreshing || component is not BeltInventory inventory)
        {
            return;
        }

        inventory.WarnWhenStuck = value;
    }
}
