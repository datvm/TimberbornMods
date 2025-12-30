namespace ModdableWeathers.UI.Settings;

public class GeneralWeatherSettingsPanel : CollapsiblePanel
{

    public GeneralWeatherSettingsPanel(GeneralWeatherSettings s, ILoc t, IContainer container)
    {
        this.BorderAndSpace();
        SetTitle(t.T("LV.MW.GenericSettings"));
        SetExpand(false);

        var props = ((IBaseWeatherSettings)s).Properties;
        foreach (var prop in props)
        {
            var el = container.GetInstance<SettingElement>();
            el.Init(prop, s);
            Container.Add(el);
        }
    }

}
