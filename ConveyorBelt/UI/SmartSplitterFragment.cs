namespace ConveyorBelt.UI;

[BindFragment]
public class SmartSplitterFragment(
    ILoc t,
    BeltGoodService goods,
    VisualElementInitializer veInit,
    DropdownItemsSetter dropdownItemsSetter
) : BaseEntityPanelFragment<SmartSplitter>
{
    readonly DropdownRow<string>[] rows = new DropdownRow<string>[3];
    bool refreshing;

    protected override void InitializePanel()
    {
        List<string> values = ["", "*"];
        List<string> labels = [t.T("LV.CBlt.Any"), t.T("LV.CBlt.Overflow")];
        foreach (var good in goods.CarryableGoods())
        {
            values.Add(good.Id);
            labels.Add(good.DisplayName.Value);
        }

        string[] titles = [t.T("LV.CBlt.Left"), t.T("LV.CBlt.Center"), t.T("LV.CBlt.Right")];
        for (var i = 0; i < rows.Length; i++)
        {
            var index = i;
            var row = new DropdownRow<string>(veInit, dropdownItemsSetter);
            row.SetLabel(titles[i]);
            row.SetItems(values, id => labels[values.IndexOf(id)]);
            row.OnValueChanged += (_, e) => OnPort(index, e.Item.Value);
            panel.Add(row);
            rows[i] = row;
        }
    }

    public override void ShowFragment(BaseComponent entity)
    {
        base.ShowFragment(entity);
        Refresh();
    }

    public override void UpdateFragment() => Refresh();

    void Refresh()
    {
        if (component is not SmartSplitter splitter)
        {
            panel.Visible = false;
            return;
        }

        refreshing = true;
        panel.Visible = true;
        var ports = splitter.SplitPorts;
        for (var i = 0; i < rows.Length && i < ports.Count; i++)
        {
            rows[i].SetSelectedValueWithoutNotifying(ports[i].Serialize());
        }

        refreshing = false;
    }

    void OnPort(int index, string value)
    {
        if (refreshing || component is not SmartSplitter splitter)
        {
            return;
        }

        splitter.SetSplitPort(index, SmartSplitPort.Deserialize(value));
    }
}
