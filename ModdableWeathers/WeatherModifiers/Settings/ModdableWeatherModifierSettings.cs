namespace ModdableWeathers.WeatherModifiers.Settings;

public class ModdableWeatherModifierSettings : IBaseWeatherSettings
{

    [Description("LV.MW.ModifierWeatherSettings")]
    public Dictionary<string, ModdableWeatherModifierWeatherSettings> Weathers { get; set; } = [];

}

public class ModdableWeatherModifierWeatherSettings
{
    [Description("LV.MW.ModifierWeatherEnabled")]
    public bool Enabled { get; set; } = true;

    [Description("LV.MW.ModifierWeatherChance")]
    public int Chance { get; set; } = 100;

    [Description("LV.MW.ModifierStartCycle")]
    public int StartCycle { get; set; }

    public bool Lock { get; set; }

}
