namespace WirelessCoil.UI;

public class WirelessCoilFragment(
    ILoc t,
    EntityBadgeService entityBadgeService,
    EntitySelectionService entitySelectionService
) : IEntityPanelFragment
{

#nullable disable
    EntityPanelFragmentElement panel;
    Label lblRange;
    VisualElement lstConnected;
#nullable enable
    WirelessCoilComponent? comp;

    public void ClearFragment()
    {
        lstConnected.Clear();
        panel.Visible = false;
        comp = null;
    }

    public VisualElement InitializeFragment()
    {
        panel = new() { Visible = false, };

        lblRange = panel.AddGameLabel().SetMarginBottom(5);

        panel.AddGameLabel(t.T("LV.WiC.InRange"));
        lstConnected = panel
            .AddScrollView(additionalClasses: ["game-scroll-view"])
            .SetMaxHeight(200);

        return panel;
    }

    public void ShowFragment(BaseComponent entity)
    {
        comp = entity.GetComponentFast<WirelessCoilComponent>();
        if (!comp)
        {
            comp = null;
            return;
        }

        ShowContent();
        panel.Visible = true;
    }

    void ShowContent()
    {
        lblRange.text = t.T("LV.WiC.Range", comp!.Range);

        lstConnected.Clear();
        foreach (var c in comp.ConnectedCoils)
        {
            var name = entityBadgeService.GetEntityName(c);
            lstConnected.AddGameButton(
                name,
                onClick: () => entitySelectionService.SelectAndFocusOn(c),
                stretched: true)
                .SetPadding(paddingY: 5)
                .SetMarginBottom(5);
        }
    }

    public void UpdateFragment()
    {
    }
}
