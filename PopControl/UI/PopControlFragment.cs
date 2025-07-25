namespace PopControl.UI;

public class PopControlFragment(
    ILoc t,
    PopControlDialog diag
) : IEntityPanelFragment
{

#nullable disable
    EntityPanelFragmentElement panel;
    Label lblBlocker;
#nullable enable

    DistrictBuilding? comp;
    BasePopControlBlocker? blocker;

    public void ClearFragment()
    {
        panel.Visible = false;
        lblBlocker.SetDisplay(false);
        comp = null;
    }

    public VisualElement InitializeFragment()
    {
        panel = new()
        {
            Visible = false,
        };

        panel.AddGameButton(t.T("LV.Pop.PopControl"), stretched: true, onClick: ShowDialog)
            .SetPadding(0, 10)
            .SetMarginBottom();

        lblBlocker = panel.AddGameLabel(t.T("LV.Pop.PopBlocked").Color(TimberbornTextColor.Red))
            .SetDisplay(false);

        return panel;
    }

    public void ShowFragment(BaseComponent entity)
    {
        comp = entity.GetComponentFast<DistrictBuilding>();
        if (!IsValidBuilding(comp))
        {
            comp = null;
            return;
        }

        blocker = comp.GetComponentFast<BasePopControlBlocker>();
        UpdateFragment();

        panel.Visible = true;
    }

    public void UpdateFragment()
    {
        if (!blocker) { return; }

        lblBlocker.SetDisplay(blocker.IsBlocking);
    }

    static bool IsValidBuilding(DistrictBuilding? bld)
    {
        if (!bld) { return false; }

        if (bld.GetComponentFast<DistrictCenter>()) { return true; }
        if (bld.GetComponentFast<Dwelling>()) { return true; }
        if (bld.GetComponentFast<BotManufactory>()) { return true; }
        if (bld.GetComponentFast<BreedingPod>()) { return true; }

        return false;
    }

    async void ShowDialog()
    {
        await diag.ShowAsync();
    }

}
