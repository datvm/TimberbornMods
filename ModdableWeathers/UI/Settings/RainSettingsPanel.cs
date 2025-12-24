namespace ModdableWeathers.UI.Settings;

public class RainSettingsPanel : CollapsiblePanel
{

    public RainSettingsPanel(ILoc t, RainSettings settings)
    {
        this.BorderAndSpace();

        SetTitle(t.T("LV.MW.RainEff"));
        SetExpand(false);

        var panel = Container;
        panel.AddLabel(t.T("LV.MW.RainEffDesc")).SetMarginBottom();

        panel.AddToggle(t.T("LV.MW.RainEffEnable"),
            onValueChanged: v => settings.RainEnabled = v)
            .SetMarginBottom()
            .SetValueWithoutNotify(settings.RainEnabled);

        var intensity = panel.AddChild().SetMarginBottom();
        intensity.AddLabel(t.T("LV.MW.RainEffIntensity"));
        intensity.AddFloatField(changeCallback: v => settings.RainIntensity = v)
            .SetValueWithoutNotify(settings.RainIntensity);
        intensity.AddLabel(t.T("LV.MW.RainEffIntensityDesc"));

        var maxParticles = panel.AddChild().SetMarginBottom();
        maxParticles.AddLabel(t.T("LV.MW.RainEffMaxParticles"));
        maxParticles.AddIntField(changeCallback: v => settings.MaxRainParticles = v)
            .SetValueWithoutNotify(settings.MaxRainParticles);
        maxParticles.AddLabel(t.T("LV.MW.RainEffMaxParticlesDesc"));
    }

}
