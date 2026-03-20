
namespace ModdableWeathers.WeatherModifiers.Settings;

public class ModdableWeatherModifierSettings : IBaseWeatherSettings
{

    public bool FirstLoad { get; set; }

    [Description("LV.MW.ModifierWeatherSettings")]
    public Dictionary<string, ModdableWeatherModifierWeatherSettings> Weathers { get; set; } = [];

    public virtual void InitializeNewSettings() { }

}

public class ModdableWeatherModifierWeatherSettings : IBaseWeatherSettings
{

    public bool FirstLoad { get; set; }

    [Description("LV.MW.ModifierWeatherEnabled")]
    public bool Enabled { get; set; }

    [Description("LV.MW.ModifierWeatherChance")]
    public int Chance { get; set; }

    [Description("LV.MW.ModifierStartCycle")]
    public int StartCycle { get; set; }

    public bool Lock { get; set; }

    JObject IBaseWeatherSettings.Serialize() => Lock ? [] : IBaseWeatherSettings.DefaultSerialize(this);

}
