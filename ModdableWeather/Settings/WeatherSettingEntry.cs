namespace ModdableWeather.Settings;

public abstract class WeatherSettingEntry : WeatherEntitySettingEntry
{

    [Description("LV.MW.EnableWeather")]
    public bool Enabled { get; set; } = true;

    [Description("LV.MW.StartCycle")]
    public int StartCycle { get; set; }

    [Description("LV.MW.Chance")]
    public int Chance { get; set; } = 100;

    [Description("LV.MW.MinDay")]
    public int MinDay { get; set; } = 5;

    [Description("LV.MW.MaxDay")]
    public int MaxDay { get; set; } = 8;

    [Description("LV.MW.HandicapPerc")]
    public int HandicapPerc { get; set; }

    [Description("LV.MW.HandicapCycles")]
    public int HandicapCycles { get; set; }

}