namespace HydroFormaProjects.UI;

public class FloodgateAutoFragment(
    ILoc t,
    FloodgateAutoService floodgateAutoService
) : IEntityPanelFragment
{
#nullable disable
    EntityPanelFragmentElement panel;
    Toggle chkSetOnHazard, chkSetOnNewCycle;
    WaterHeightSlider heightHazard, heightNewCycle;
#nullable enable

    FloodgateAutoComponent? comp;

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

        AddHeightPanel(panel, "LV.HF.FloodgateSetOnHaz", ref chkSetOnHazard, ref heightHazard,
            e => OnHeightEnabled(true, e),
            h => OnHeightChanged(true, h)
        );

        AddHeightPanel(panel, "LV.HF.FloodgateSetOnNewCycle", ref chkSetOnNewCycle, ref heightNewCycle,
            e => OnHeightEnabled(false, e),
            h => OnHeightChanged(false, h)
        );

        return panel;
    }

    void AddHeightPanel(VisualElement parent, string locKey,
        ref Toggle toggle, ref WaterHeightSlider slider,
        Action<bool> onChecked, Action<float> onHeightChanged)
    {
        var container = parent.AddChild().SetMarginBottom(10);

        toggle = container.AddToggle(t.T(locKey), onValueChanged: onChecked);
        slider = container.AddChild<WaterHeightSlider>()
            .RegisterHeightChange(onHeightChanged);
    }

    public void ShowFragment(BaseComponent entity)
    {
        comp = entity.GetComponentFast<FloodgateAutoComponent>();
        if (comp == null || !floodgateAutoService.IsUnlocked)
        {
            comp = null;
            return;
        }

        UpdateContent();
        panel.Visible = true;
    }

    public void UpdateFragment() { }

    void OnHeightEnabled(bool hazard, bool enabled)
    {
        if (!comp) { return; }
        if (hazard)
        {
            comp.SetOnHazard = enabled;
        }
        else
        {
            comp.SetOnNewCycle = enabled;
        }
        UpdateContentEnabled();
    }

    void OnHeightChanged(bool hazard, float height)
    {
        if (!comp) { return; }

        if (hazard)
        {
            comp.HeightOnHazard = height;
        }
        else
        {
            comp.HeightOnNewCycle = height;
        }
    }

    void UpdateContent()
    {
        chkSetOnHazard.SetValueWithoutNotify(comp!.SetOnHazard);
        heightHazard.SetValues(comp.HeightOnHazard, comp.MaxHeight);

        chkSetOnNewCycle.SetValueWithoutNotify(comp.SetOnNewCycle);
        heightNewCycle.SetValues(comp.HeightOnNewCycle, comp.MaxHeight);

        UpdateContentEnabled();
    }

    void UpdateContentEnabled()
    {
        heightHazard.enabledSelf = comp!.SetOnHazard;
        heightNewCycle.enabledSelf = comp.SetOnNewCycle;
    }

}
