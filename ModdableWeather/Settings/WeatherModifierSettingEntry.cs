namespace ModdableWeather.Settings;

public abstract class WeatherModifierSettingEntry : WeatherEntitySettingEntry
{

    [Description("LV.MW.EnableModifier")]
    public bool Enabled { get; set; } = true;

    [Description("LV.MW.StartCycle")]
    public int StartCycle { get; set; }

    [Description("LV.MW.Chance")]
    public int Chance { get; set; } = 100;

    [Description("LV.MW.AssociatedWeathers")]
    public HashSet<string> AssociatedWeathers { get; set; } = [];

}
