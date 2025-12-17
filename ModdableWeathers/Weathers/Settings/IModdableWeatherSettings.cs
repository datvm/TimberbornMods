namespace ModdableWeathers.Weathers.Settings;

public interface IModdableWeatherSettings : IBaseWeatherSettings
{
    bool CanSupport(GameModeSpec gameMode);
    void SetTo(GameModeSpec gameMode);
}

public interface IDefaultModdableWeatherSettings : IModdableWeatherSettings
{
    int Chance { get; set; }
    bool Enabled { get; set; }
    int HandicapCycles { get; set; }
    int HandicapPercent { get; set; }
    int MaxDay { get; set; }
    int MinDay { get; set; }
    int StartCycle { get; set; }
}