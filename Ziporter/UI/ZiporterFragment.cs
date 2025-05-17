namespace Ziporter.UI;

public class ZiporterFragment(
    ILoc t,
    VisualElementInitializer veInit
) : IEntityPanelFragment
{

#nullable disable
    EntityPanelFragmentElement panel;
    Timberborn.CoreUI.ProgressBar pgbStabilizer;
    Label lblStabilizer;
#nullable enable
    ZiporterController? comp;

    public void ClearFragment()
    {
        comp = null;
        panel.Visible = false;
    }

    public VisualElement InitializeFragment()
    {
        panel = new()
        {
            Background = EntityPanelFragmentBackground.Green,
            Visible = true,
        };

        AddStabilizerUI(panel);

        return panel.Initialize(veInit);
    }

    void AddStabilizerUI(EntityPanelFragmentElement panel)
    {
        pgbStabilizer = panel.AddProgressBar()
            .SetColor(ProgressBarColor.Red)
            .SetMarginBottom();

        lblStabilizer = pgbStabilizer.AddProgressLabel();
    }

    public void ShowFragment(BaseComponent entity)
    {
        comp = entity.GetComponentFast<ZiporterController>();
        if (!comp) { return; }

        UpdatePanelContent();
        panel.Visible = true;
    }

    void UpdatePanelContent()
    {
        UpdateFragment();
    }

    public void UpdateFragment()
    {
        if (!comp) { return; }

        UpdateStabilizerStatus();
    }

    void UpdateStabilizerStatus()
    {
        var perc = comp!.StabilizerPercent;
        var isUsing = perc < 1f && !comp.IsStabilizerCharging;

        var stabilizerStatus = isUsing
            ? "LV.Ziporter.StabilizerUse"
            : (perc >= 1
                ? "LV.Ziporter.StabilizerStable"
                : "LV.Ziporter.StabilizerCharging");

        pgbStabilizer
            .SetProgress(perc, lblStabilizer, stabilizerStatus.T(t));
    }

}
