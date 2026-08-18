namespace Crane.UI;

[BindFragment]
public class CraneFragment(
    ILoc t,
    DialogService diag,
    CraneStructureService structureService
) : BaseEntityPanelFragment<CraneComponent>
{
    Label height = null!;
    Button buildHigher = null!;

    protected override void InitializePanel()
    {
        height = panel.AddGameLabel().SetMarginBottom();
        buildHigher = panel.AddGameButtonPadded(t.T("LV.Cr.BuildHigher"), BuildHigher).SetFlexGrow();
    }

    public override void ShowFragment(BaseComponent entity)
    {
        base.ShowFragment(entity);
        if (!component || !component!.IsFinished)
        {
            panel.Visible = false;
            return;
        }

        buildHigher.SetEnabled(structureService.CanBuildHigher(component));
    }

    public override void UpdateFragment()
    {
        base.UpdateFragment();
        if (!component || !component!.IsFinished)
        {
            return;
        }

        UpdateHeight();
    }

    void UpdateHeight()
    {
        var tower = component!.Tower;
        height.text = tower.TargetHeight > tower.Height
            ? t.T("LV.Cr.HeightPending", tower.Height, tower.TargetHeight)
            : t.T("LV.Cr.Height", tower.Height);
    }

    void BuildHigher()
    {
        if (!component) { return; }

        if (!structureService.TryBuildHigher(component!))
        {
            diag.Alert("LV.Cr.CannotBuildHigher", true);
            buildHigher.SetEnabled(false);
            return;
        }

        UpdateHeight();
        buildHigher.SetEnabled(structureService.CanBuildHigher(component!));
    }

}
