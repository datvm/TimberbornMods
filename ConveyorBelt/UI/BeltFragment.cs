namespace ConveyorBelt.UI;

[BindFragment]
public class BeltFragment(ILoc t) : BaseEntityPanelFragment<BeltCarrier>
{
    Toggle warn = null!;
    Label goods = null!;
    bool refreshing;

    protected override void InitializePanel()
    {
        warn = panel.AddGamePanelToggle(t.T("LV.CBlt.WarnStuck"), OnWarn).SetMarginBottom(5);
        goods = panel.AddGameLabel();
    }

    public override void ShowFragment(BaseComponent entity)
    {
        base.ShowFragment(entity);
        Refresh();
    }

    public override void UpdateFragment() => Refresh();

    void Refresh()
    {
        if (component is not BeltCarrier belt)
        {
            return;
        }

        refreshing = true;
        warn.SetValueWithoutNotify(belt.WarnWhenStuck);
        goods.text = Describe(belt.Items);
        refreshing = false;
    }

    void OnWarn(bool value)
    {
        if (refreshing || component is not BeltCarrier belt)
        {
            return;
        }

        belt.WarnWhenStuck = value;
    }

    string Describe(IReadOnlyList<BeltGood> items)
    {
        if (items.Count == 0)
        {
            return t.T("LV.CBlt.Empty");
        }

        var text = "";
        foreach (var item in items)
        {
            if (text.Length > 0)
            {
                text += ", ";
            }

            text += item.Id;
        }

        return t.T("LV.CBlt.Carrying", text);
    }
}
