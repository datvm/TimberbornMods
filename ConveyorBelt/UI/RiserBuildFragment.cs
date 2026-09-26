namespace ConveyorBelt.UI;

[BindFragment]
public class RiserBuildFragment(ILoc t, RiserPlacer placer, FactionService factions, ConveyorBeltSpeeds speeds) : BaseEntityPanelFragment<BeltTeleporter>
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
        panel.Visible = component is BeltTeleporter lift && lift.Kind is TeleporterKind.LiftUp or TeleporterKind.LiftDown;
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
        if (component is not BeltTeleporter lift)
        {
            return;
        }

        placer.TryPlaceAbove(lift.GetComponent<BlockObject>(), speedId, shape);
    }
}
