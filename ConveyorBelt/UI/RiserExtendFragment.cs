namespace ConveyorBelt.UI;

[BindFragment]
public class RiserExtendFragment(ILoc t, RiserPlacer placer, FactionService factions, ConveyorBeltSpeeds speeds) : BaseEntityPanelFragment<BeltCarrier>
{
    protected override void InitializePanel()
    {
        panel.AddGameLabel(t.T("LV.CBlt.BuildRiser")).SetMarginBottom(5);
        foreach (var level in speeds.Levels)
        {
            AddRow(level);
        }
    }

    public override void ShowFragment(BaseComponent entity)
    {
        base.ShowFragment(entity);
        panel.Visible = component is BeltCarrier riser && riser.Shape is BeltShape.RiserUp or BeltShape.RiserDown;
    }

    public override void UpdateFragment()
    {
        if (component is not BeltCarrier riser)
        {
            return;
        }

        panel.Visible = riser.Shape is BeltShape.RiserUp or BeltShape.RiserDown;
    }

    void AddRow(ConveyorBeltSpeedLevel level)
    {
        var row = panel.AddRow().AlignItems().SetMarginBottom(5);
        row.AddGameLabel(t.T(level.NameKey(factions.Current.Id))).SetFlexGrow(1).SetMarginRight(5);
        row.AddGameButton(t.T("LV.CBlt.Up"), () => Place(level.Id, BeltShape.RiserUp)).SetMarginRight(5);
        row.AddGameButton(t.T("LV.CBlt.Down"), () => Place(level.Id, BeltShape.RiserDown));
    }

    void Place(string speedId, BeltShape shape)
    {
        if (component is not BeltCarrier riser)
        {
            return;
        }

        placer.TryPlaceAbove(riser.GetComponent<BlockObject>(), speedId, shape);
    }
}
