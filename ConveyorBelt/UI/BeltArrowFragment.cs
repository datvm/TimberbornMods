namespace ConveyorBelt.UI;

[BindFragment]
public class BeltArrowFragment(ILoc t, BeltArrowRepository arrows) : BaseEntityPanelFragment<BeltArrows>
{
    Toggle show = null!;

    protected override void InitializePanel() => show = panel.AddGamePanelToggle(t.T("LV.CBlt.ShowArrows"), OnShow);

    public override void ShowFragment(BaseComponent entity)
    {
        base.ShowFragment(entity);
        if (component is not BeltArrows)
        {
            return;
        }

        show.SetValueWithoutNotify(arrows.ShowArrows);
    }

    void OnShow(bool value) => arrows.SetShowArrows(value);
}
