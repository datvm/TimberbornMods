namespace ModdableWeathers.UI.Settings;

public class WeatherSettingsPanel(ILoc t, IContainer container)
    : BaseWeatherSettingsPanel<IModdableWeather, IModdableWeatherSettings>(t, container)
{
    protected override string GetTitle() => Entity.Spec.Display.Value;
    protected override string GetDescription() => Entity.Spec.Description.Value;

    protected override IModdableWeatherSettings? GetSettings() 
        => Entity is IModdableWeatherWithSettings ws ? ws.Settings : null;

    protected override bool Match(WeatherSettingsDialogFilter filter) => Entity.Match(filter);

}
