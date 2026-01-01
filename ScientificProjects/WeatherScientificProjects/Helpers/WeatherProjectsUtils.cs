namespace WeatherScientificProjects.Helpers;

public static class WeatherProjectsUtils
{

    public const string WeatherDecreaseStrBad = "WeatherDecreaseStrBad";
    public const string WeatherDecreaseStrFresh = "WeatherDecreaseStrFresh";
    public const string WeatherIncreaseStrBad = "WeatherIncreaseStrBad";
    public const string WeatherIncreaseStrFresh = "WeatherIncreaseStrFresh";

    public const string WeatherForecast3Id = "WeatherPred3";
    public const string WeatherWarningExt3Id = "WeatherExt3";

    public static readonly FrozenSet<string> EmergencyDrillIds = ["EmergencyDrill1", "EmergencyDrill2", "EmergencyDrill3"];

    public static readonly FrozenSet<string> FreshWaterStrengthIds = [WeatherDecreaseStrFresh, WeatherIncreaseStrFresh];
    public static readonly FrozenSet<string> BadWaterStrengthIds = [WeatherDecreaseStrBad, WeatherIncreaseStrBad];
    public static readonly FrozenSet<string> WaterStrengthIncreaseIds = [WeatherIncreaseStrBad, WeatherIncreaseStrFresh];

    public static readonly FrozenSet<string> PrewarningIds = [WeatherForecast3Id, WeatherWarningExt3Id];

    public static readonly FrozenSet<string> WeatherWarningExtUnlockIds = ["WeatherExt1", "WeatherExt2", ];
    public static readonly FrozenSet<string> WeatherForecastUnlockIds = ["WeatherPred1", "WeatherPred2", ];

}

public enum GameWeatherStage
{
    Temperate,
    Warning,
    Hazardous,    
}