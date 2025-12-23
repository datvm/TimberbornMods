namespace ModdableWeathers.UI.Settings;

public class WeatherModifierAssociationPanel(
    ModdableWeatherRegistry weatherRegistry,
    IContainer container,
    ILoc t
) : CollapsiblePanel
{

    public IModdableWeather? Weather { get; private set; }
    Label? lblDisabled;

    public void Init(string weatherId, ModdableWeatherModifierWeatherSettings settings)
    {
        this
            .SetMarginBottom(10)
            .SetPadding(5)
            .SetBorder(color: TextColors.YellowHighlight, width: 1);

        Weather = weatherRegistry.WeathersById.GetOrDefault(weatherId);
        if (Weather is null)
        {
            SetTitle(t.T("LV.MW.UnknownWeather", weatherId));
            SetExpand(false);
            return;
        }

        SetTitle(Weather.Spec.Display.Value);
        lblDisabled = AppendHeaderLabel(t.T("LV.MW.SettingsDisabled"));

        var panel = Container.AddChild();

        var properties = ((IBaseWeatherSettings)settings).Properties;
        foreach (var property in properties)
        {
            var el = container.GetInstance<SettingElement>();
            el.Init(property, settings);

            if (el.IsEnabledProperty)
            {
                el.OnEnabledChanged += e => lblDisabled.SetDisplay(!e);
                lblDisabled.SetDisplay(!(bool)property.Property.GetValue(settings));
            }

            panel.Add(el);
        }

        if (settings.Lock)
        {
            panel.enabledSelf = false;
            AppendHeaderLabel(t.T("LV.MW.SettingsLocked"));
        }
    }

    Label AppendHeaderLabel(string text)
    {
        var label = this.AddLabel(text);
        label.InsertSelfAfter(HeaderLabel);
        return label;
    }

}
