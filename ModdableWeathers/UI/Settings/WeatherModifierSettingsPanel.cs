
namespace ModdableWeathers.UI.Settings;

public class WeatherModifierSettingsPanel(
    ModdableWeatherRegistry weatherRegistry,
    ILoc t, IContainer container) : BaseWeatherSettingsPanel<IModdableWeatherModifier, ModdableWeatherModifierSettings>(t, container)
{
    protected override string GetTitle() => Entity.Spec.Name.Value;
    protected override string GetDescription() => Entity.Spec.Description.Value;

    protected override ModdableWeatherModifierSettings? GetSettings() => Entity.Settings;

    protected override bool Match(WeatherSettingsDialogFilter filter)
    {
        if (filter.Query.Length > 0 || Entity.Spec.Name.Value.Contains(filter.Query, StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        foreach (var id in Settings.Weathers.Keys)
        {
            var weather = weatherRegistry.GetOrDefault(id);
            if (weather is not null && weather.Match(filter))
            {
                return true;
            }
        }

        return false;
    }
}
