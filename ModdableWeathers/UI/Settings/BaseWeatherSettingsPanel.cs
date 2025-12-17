namespace ModdableWeathers.UI.Settings;

public abstract class BaseWeatherSettingsPanel<T, TSettings> : CollapsiblePanel, IFilterablePanel
    where TSettings : IBaseWeatherSettings
{
    public readonly T Entity;
    public readonly TSettings Settings = default!;

    public BaseWeatherSettingsPanel(T entity, ILoc t)
    {
        Entity = entity;

        SetTitle(GetTitle());
        SetExpand(false);
        this.SetMarginBottom(10);

        var parent = Container;
        parent.AddLabel(GetDescription()).SetMarginBottom();

        var s = GetSettings();
        if (s is null)
        {
            parent.AddLabel(t.T("LV.MW.NoSettings"));
            return;
        }
        Settings = s;

        PopulateProperties(parent, t);
    }

    void PopulateProperties(VisualElement parent, ILoc t)
    {
        var list = parent.AddChild();

        var props = Settings.Properties;

        foreach (var prop in props)
        {
            var el = new SettingElement(prop, Settings, t);
            list.Add(el);
        }
    }

    public void Filter(WeatherSettingsDialogFilter filter) => this.SetDisplay(Match(filter));
    protected abstract bool Match(WeatherSettingsDialogFilter filter);

    protected abstract TSettings? GetSettings();

    protected abstract string GetTitle();
    protected abstract string GetDescription();

}
