
namespace ModdableWeathers.WeatherModifiers.Settings;

public class ModdableWeatherModifierSettings : IBaseWeatherSettings
{

    [Description("LV.MW.ModifierWeatherSettings")]
    public Dictionary<string, ModdableWeatherModifierWeatherSettings> Weathers { get; set; } = [];

}

public class ModdableWeatherModifierWeatherSettings : IBaseWeatherSettings
{
    [Description("LV.MW.ModifierWeatherEnabled")]
    public bool Enabled { get; set; }

    [Description("LV.MW.ModifierWeatherChance")]
    public int Chance { get; set; }

    [Description("LV.MW.ModifierStartCycle")]
    public int StartCycle { get; set; }

    public bool Lock { get; set; }

    JObject IBaseWeatherSettings.Serialize() => Lock ? [] : IBaseWeatherSettings.DefaultSerialize(this);

}
