namespace ModdableWeathers.UI.Settings;

public class GlobalSettingsPanel : CollapsiblePanel
{

    public GlobalSettingsPanel(ILoc t, GeneralGlobalSettings settings, IContainer container)
    {
        this.BorderAndSpace();
        SetTitle(t.T("LV.MW.GlobalSettings"));

        var parent = Container;
        parent.AddLabel(t.T("LV.MW.GlobalSettingsDesc")).SetMarginBottom();

        var props = ((IBaseWeatherSettings)settings).Properties;
        foreach (var prop in props)
        {
            var el = container.GetInstance<SettingElement>();
            el.Init(prop, settings);
            parent.Add(el);
        }

    }

}
