namespace HydroFormaProjects.UI;

public class StreamGaugeUpgradeFragment(
    ILoc t,
    StreamGaugeSensorService service,
    IDayNightCycle dayNight
) : IEntityPanelFragment
{

#nullable disable
    EntityPanelFragmentElement panel;

    Label lblDepth;
    Button btnMeasure;
    Toggle chkMeasureFullCube;
    Label lblVolume;
#nullable enable

    StreamGaugeSensor? comp;

    public void ClearFragment()
    {
        panel.Visible = false;
        comp = null;
    }

    public VisualElement InitializeFragment()
    {
        panel = new()
        {
            Visible = false,
        };

        lblDepth = panel.AddGameLabel().SetMarginBottom();

        btnMeasure = panel.AddGameButton(onClick: MeasureVolume).SetWidthPercent(100);
        var row = btnMeasure.AddRow().AlignItems().JustifyContent();

        row.AddGameLabel(t.T("LV.HF.SGMeasureVolume")).SetMarginRight(5);
        row.AddGameLabel(service.MeasureVolumeScienceCost.ToString());
        row.AddChild(classes: ["science-cost-section__science-icon"]);

        chkMeasureFullCube = panel.AddToggle(t.T("LV.HF.SGMeasureVolumeFullCube"))
            .SetWidthPercent(100)
            .SetMarginBottom(10);
        chkMeasureFullCube.SetValueWithoutNotify(true);

        lblVolume = panel.AddGameLabel().SetDisplay(false);

        return panel;
    }

    public void ShowFragment(BaseComponent entity)
    {
        comp = entity.GetComponentFast<StreamGaugeSensor>();
        if (!comp) { return; }

        if (!service.CanUseProject)
        {
            comp = null;
            return;
        }

        UpdateFragment();
        panel.Visible = true;
    }

    public void UpdateFragment()
    {
        if (!comp) { return; }

        if (comp.WaterLevel is null)
        {
            lblDepth.text = "⌛";
        }
        else
        {
            var (h, z, soilZ) = comp.WaterLevel.Value;

            lblDepth.text = string.Format(t.T("LV.HF.SGBottomDepth"),
                h - soilZ, h - z, z);
        }

        btnMeasure.enabledSelf = service.CanMeasureVolume;
        if (comp.VolumeMeasurement is null)
        {
            lblVolume.SetDisplay(false);
        }
        else
        {
            var (v, n, day) = comp.VolumeMeasurement.Value;
            lblVolume.text = string.Format(t.T("LV.HF.SGMeasureVolumeResult"),
                v, n, dayNight.DayNumber - day);
            lblVolume.SetDisplay(true);
        }
    }

    void MeasureVolume()
    {
        if (!comp) { return; }

        service.MeasureVolumeRequested(comp, chkMeasureFullCube.value);
    }
}
