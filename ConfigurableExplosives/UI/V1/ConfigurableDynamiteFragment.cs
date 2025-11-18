namespace ConfigurableExplosives.UI;

public class ConfigurableDynamiteFragment(
    ILoc t,
    VisualElementInitializer veInit
) : BaseEntityPanelFragment<ConfigurableDynamiteComponent>
{

#nullable disable
    GameSliderInt delaySlider, depthSlider, triggerRadius;
    Label lblDetonateIn;
#nullable enable

    protected override void InitializePanel()
    {
        delaySlider = AddSlider(0, 100,
            t.T("LV.CE.Delay"), v => t.T("LV.CE.DelayDisplay", (v / 10f).ToString("0.0")),
            OnDelayChanged);
        depthSlider = AddSlider(1, 1,
            t.T("LV.CE.Depth"),
            v => t.T("LV.CE.DepthDisplay", v),
            OnDepthChanged);
        triggerRadius = AddSlider(0, 20,
            t.T("LV.CE.TriggerRadius"), v => t.T("LV.CE.TriggerRadiusDisplay", v),
            OnTriggerRadiusChanged);
        lblDetonateIn = panel.AddGameLabel();

        panel.Initialize(veInit);

        GameSliderInt AddSlider(int min, int max, string key, Func<int, string> display, Action<int> onChange)
        {
            var slider = panel.AddSliderInt(key, values: new(min, max, 0));

            slider.RegisterChange(onChange)
                .AddEndLabel(display);

            return slider;
        }
    }

    public override void ShowFragment(BaseComponent entity)
    {
        base.ShowFragment(entity);

        if (component is null) { return; }

        lblDetonateIn.ToggleDisplayStyle(false);
        UpdateValues();
    }

    void UpdateValues()
    {
        depthSlider.Slider.highValue = component!.MaxDepth;
        depthSlider.SetValue(component.DetonationDepth);

        delaySlider.SetValue(Mathf.RoundToInt(component.DetonationDelay * 10f));
        triggerRadius.SetValue(component.TriggerRadius);

        UpdateDetonateLabel();
    }

    void UpdateDetonateLabel()
    {
        if (component?.Triggered != true) { return; }

        lblDetonateIn.ToggleDisplayStyle(true);
        lblDetonateIn.text = t.T("LV.CE.DetonateIn",
            component.TriggeredTime.ToString("0.0"),
            component.DetonationDelay.ToString("0.0"));
    }

    void OnDepthChanged(int value)
    {
        if (!component) { return; }
        component!.DetonationDepth = value;
    }

    void OnDelayChanged(int value)
    {
        if (!component) { return; }

        component!.DetonationDelay = value / 10f;
    }

    void OnTriggerRadiusChanged(int value)
    {
        if (!component) { return; }
        component!.TriggerRadius = value;
    }

    public override void UpdateFragment()
    {
        UpdateDetonateLabel();
    }

}
