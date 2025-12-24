namespace ModdableWeathers.UI.Settings;

public abstract class BaseWeatherSettingsPanel<T, TSettings>(ILoc t, IContainer container) : CollapsiblePanel, IFilterablePanel
    where TSettings : IBaseWeatherSettings
{
#nullable disable
    public T Entity { get; private set; }
    public TSettings Settings { get; private set; }
    Label lblDisabled;
#nullable enable

    public virtual void Init(T entity)
    {
        Entity = entity;

        SetTitle(GetTitle());
        SetExpand(false);
        this.BorderAndSpace();

        lblDisabled = this.AddLabel(t.T("LV.MW.SettingsDisabled"));
        lblDisabled.InsertSelfAfter(HeaderLabel);
        lblDisabled.SetDisplay(false);

        var parent = Container;
        parent.AddLabel(GetDescription()).SetMarginBottom();

        var s = GetSettings();
        if (s is null)
        {
            parent.AddLabel(t.T("LV.MW.NoSettings"));
            return;
        }
        Settings = s;

        PopulateProperties(parent);
    }

    void PopulateProperties(VisualElement parent)
    {
        var list = parent.AddChild();

        var props = Settings.Properties;

        foreach (var prop in props)
        {
            var el = container.GetInstance<SettingElement>();
            el.Init(prop, Settings);

            if (el.IsEnabledProperty)
            {
                el.OnEnabledChanged += enabled => lblDisabled.SetDisplay(!enabled);
                lblDisabled.SetDisplay(!(bool)prop.Property.GetValue(Settings));
            }

            list.Add(el);
        }
    }

    public void Filter(WeatherSettingsDialogFilter filter) => this.SetDisplay(Match(filter));
    protected abstract bool Match(WeatherSettingsDialogFilter filter);

    protected abstract TSettings? GetSettings();

    protected abstract string GetTitle();
    protected abstract string GetDescription();

}
