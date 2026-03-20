namespace ModdableWeathers.UI.Settings;

public class GlobalSettingsPanel : CollapsiblePanel
{
    readonly InitialWeatherSettingsService initialService;

    public GlobalSettingsPanel(ILoc t, GeneralGlobalSettings settings, IContainer container, InitialWeatherSettingsService initialService)
    {
        this.initialService = initialService;

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

        var defSettings = parent.AddChild().SetMarginBottom(10);
        defSettings.AddLabel(t.T("LV.MW.DefaultSettings"));
        defSettings.AddLabel(t.T("LV.MW.DefaultSettingsDesc")).SetMarginBottom(5);

        var defButtons = defSettings.AddRow();
        defButtons.AddMenuButton(t.T("LV.MW.DefaultUseCurrent"), onClick: SetDefaultCurrentSettings).SetFlexGrow(1);
        defButtons.AddMenuButton(t.T("KeyBindingBox.ClearBinding"), onClick: ClearDefaultCurrentSettings).SetFlexGrow(1);
    }

    void SetDefaultCurrentSettings()
    {
        initialService.UseCurrent();
    }

    void ClearDefaultCurrentSettings()
    {
        initialService.ClearDefault();
    }

}
