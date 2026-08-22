namespace Crane.UI;

[BindFragment]
public class CraneSectionFragment(
    ILoc t,
    CraneStructureService craneStructureService
) : IEntityPanelFragment
{
    EntityPanelFragmentElement panel = null!;
    ICranePartComponent? part;

    public VisualElement InitializeFragment()
    {
        panel = new();
        panel.AddEntityFragmentButton(t.T("LV.Cr.SelectCrane"), SelectCrane, color: EntityFragmentButtonColor.Red);
        return panel;
    }

    public void ShowFragment(BaseComponent entity)
    {
        part = entity.GetComponent<ICranePartComponent>();
        panel.Visible = part?.GetCrane() is { } crane && crane;
    }

    public void ClearFragment()
    {
        panel.Visible = false;
        part = null;
    }

    public void UpdateFragment() { }

    void SelectCrane() => craneStructureService.SelectCrane(part?.GetCrane());
}
