namespace Ziporter.UI;

public class ZiporterFragment(
    ILoc t,
    VisualElementInitializer veInit,
    DevModeManager devModeManager,
    VisualElementLoader veLoader,
    ZiporterConnectionButtonFactory btnFac
) : IEntityPanelFragment
{

#nullable disable
    EntityPanelFragmentElement panel;
    Timberborn.CoreUI.ProgressBar pgbStabilizer;
    Label lblStabilizer;
    GameSliderInt devModeStabilizer;
    VisualElement connectionButtons;
    Label lblConnectionStatus;
#nullable enable
    ZiporterController? comp;

    public void ClearFragment()
    {
        comp = null;
        connectionButtons.Clear();
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
        AddConnectionUI(panel);

        return panel.Initialize(veInit);
    }

    void AddStabilizerUI(VisualElement parent)
    {
        devModeStabilizer = parent.AddSliderInt(values: new(0, 100, 100))
            .RegisterChange(SetStabilizer);

        pgbStabilizer = parent.AddProgressBar()
            .SetColor(ProgressBarColor.Red)
            .SetMarginBottom();

        lblStabilizer = pgbStabilizer.AddProgressLabel();
    }

    void AddConnectionUI(VisualElement parent)
    {
        var container = veLoader.LoadVisualElement("Game/EntityPanel/ZiplineTowerFragment")
            .SetMarginBottom(10);
        parent.Add(container);
        connectionButtons = container.Q("Buttons");

        lblConnectionStatus = parent.AddGameLabel(centered: true);
    }

    public void ShowFragment(BaseComponent entity)
    {
        comp = entity.GetComponentFast<ZiporterController>();
        if (!comp) { return; }

        if (!comp.IsFinished)
        {
            ClearFragment();
            return;
        }

        UpdatePanelContent();
        panel.Visible = true;
    }

    void UpdatePanelContent()
    {
        devModeStabilizer.SetDisplay(devModeManager.Enabled);
        CreateConnectionButton();

        UpdateFragment();
    }

    void CreateConnectionButton()
    {
        var from = comp!.Connection;
        var to = from.ConnectedZiporter;

        if (to)
        {
            btnFac.CreateConnection(connectionButtons, from, to);
        }
        else
        {
            btnFac.CreateAddConnection(connectionButtons, from);
        }
    }

    public void UpdateFragment()
    {
        if (!comp) { return; }

        UpdateStabilizerStatus();
        UpdateConnectionStatus();
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
        devModeStabilizer.SetValueWithoutNotify(Mathf.FloorToInt(perc * 100));
    }

    void SetStabilizer(int perc) => comp?.SetStabilizerPerc(perc);

    void UpdateConnectionStatus()
    {
        var status = "LV.Ziporter.ConnectionEstablished".T(t).Color(TimberbornTextColor.Green);

        if (!comp!.Connection.IsConnected)
        {
            status = "LV.Ziporter.ConnectionNone".T(t).Color(TimberbornTextColor.Solid);
        }
        else if (!comp.Connection.IsActive)
        {
            status = "LV.Ziporter.ConnectionEstablishing".T(t, ZiporterConnection.ActivateCapacity)
                .Color(TimberbornTextColor.Green);
        }
        else if (!comp.IsCharging)
        {
            status = "LV.Ziporter.ConnectionLosing".T(t, ZiporterConnection.DeactivateCapacity)
                .Color(TimberbornTextColor.Red);
        }

        lblConnectionStatus.text = status;
    }

}
