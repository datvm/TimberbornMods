namespace ConfigurableExplosives.UI;

public class ConfigurableDynamiteFragment(ILoc t, VisualElementInitializer veInit, ConfigurableDynamiteCopyTool copyTool) : IEntityPanelFragment
{

    ConfigurableDynamiteComponent? comp;

#nullable disable
    EntityPanelFragmentElement panel;
    GameSliderInt delaySlider, depthSlider, triggerRadius;
    Label lblDetonateIn;
#nullable enable

    public VisualElement InitializeFragment()
    {
        var parent = panel = new()
        {
            Visible = false,
            Background = EntityPanelFragmentBackground.Green,
        };

        delaySlider = AddSlider(parent, 0, 100,
            t.T("LV.CE.Delay"), v => t.T("LV.CE.DelayDisplay", (v / 10f).ToString("0.0")),
            OnDelayChanged);
        depthSlider = AddSlider(parent,
            1, 1, t.T("LV.CE.Depth"),
            v => t.T("LV.CE.DepthDisplay", v),
            OnDepthChanged);
        triggerRadius = AddSlider(parent,
            0, 20,
            t.T("LV.CE.TriggerRadius"), v => t.T("LV.CE.TriggerRadiusDisplay", v),
            OnTriggerRadiusChanged);
        lblDetonateIn = parent.AddGameLabel();

        parent.AddGameButton(t.T("LV.CE.Copy"), Copy, stretched: true)
            .SetMargin(top: 20);

        return parent.Initialize(veInit);
    }

    GameSliderInt AddSlider(VisualElement parent, int min, int max, string key, Func<int, string> display, Action<int> onChange)
    {
        var slider = parent.AddSliderInt(key, values: new(min, max, 0));

        slider.RegisterChange(onChange)
            .AddEndLabel(display);

        return slider;
    }

    public void ShowFragment(BaseComponent obj)
    {
        comp = obj.GetComponentFast<ConfigurableDynamiteComponent>();
        panel.ToggleDisplayStyle(comp);
        if (!comp) { return; }

        lblDetonateIn.ToggleDisplayStyle(false);
        UpdateValues();
    }

    void UpdateValues()
    {
        if (!comp) { return; }

        depthSlider.Slider.highValue = comp.MaxDepth;
        depthSlider.SetValue(comp.DetonationDepth);

        delaySlider.SetValue(Mathf.RoundToInt(comp.DetonationDelay * 10f));
        triggerRadius.SetValue(comp.TriggerRadius);

        UpdateDetonateLabel();
    }

    void UpdateDetonateLabel()
    {
        if (comp?.Triggered != true) { return; }

        lblDetonateIn.ToggleDisplayStyle(true);
        lblDetonateIn.text = t.T("LV.CE.DetonateIn",
            comp.TriggeredTime.ToString("0.0"),
            comp.DetonationDelay.ToString("0.0"));
    }

    void OnDepthChanged(int value)
    {
        if (!comp) { return; }
        comp.DetonationDepth = value;
    }

    void OnDelayChanged(int value)
    {
        if (!comp) { return; }

        comp.DetonationDelay = value / 10f;
    }

    void OnTriggerRadiusChanged(int value)
    {
        if (!comp) { return; }
        comp.TriggerRadius = value;
    }

    void Copy()
    {
        if (!comp) { return; }

        var curr = comp;
        copyTool.Activate(dynamites => PerformCopy(curr, dynamites));
    }

    void PerformCopy(ConfigurableDynamiteComponent src, ImmutableArray<ConfigurableDynamiteComponent> dynamites)
    {
        if (!src || !src.enabled) { return; }

        foreach (var d in dynamites)
        {
            if (d != src) { d.CopyFrom(src); }
        }
    }

    public void ClearFragment()
    {
        comp = null;
        panel.Visible = false;
    }

    public void UpdateFragment()
    {
        UpdateDetonateLabel();
    }

}
