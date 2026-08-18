namespace Crane.UI;

[BindFragment]
public class CraneSectionFragment(
    ILoc t,
    CraneStructureService craneStructureService
) : BaseEntityPanelFragment<CraneSectionComponent>
{
    
    protected override void InitializePanel()
    {
        panel.AddEntityFragmentButton(t.T("LV.Cr.SelectCrane"), SelectCrane, color: EntityFragmentButtonColor.Red);
    }

    void SelectCrane()
    {
        craneStructureService.SelectCraneOfSection(component);
    }

}
