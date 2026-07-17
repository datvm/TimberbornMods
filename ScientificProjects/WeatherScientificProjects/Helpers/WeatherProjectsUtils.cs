namespace WeatherScientificProjects.Helpers;

public static class WeatherProjectsUtils
{

    public const string WeatherDecreaseStrBad = "WeatherDecreaseStrBad";
    public const string WeatherDecreaseStrFresh = "WeatherDecreaseStrFresh";
    public const string WeatherIncreaseStrBad = "WeatherIncreaseStrBad";
    public const string WeatherIncreaseStrFresh = "WeatherIncreaseStrFresh";

    public const string WeatherForecast3Id = "WeatherPred3";
    public const string WeatherWarningExt3Id = "WeatherExt3";

    public static readonly FrozenSet<string> EmergencyDrillIds = ImmutableHelper.CreateFrozenSet(["EmergencyDrill1", "EmergencyDrill2", "EmergencyDrill3"]);

    public static readonly FrozenSet<string> FreshWaterStrengthIds = ImmutableHelper.CreateFrozenSet([WeatherDecreaseStrFresh, WeatherIncreaseStrFresh]);
    public static readonly FrozenSet<string> BadWaterStrengthIds = ImmutableHelper.CreateFrozenSet([WeatherDecreaseStrBad, WeatherIncreaseStrBad]);
    public static readonly FrozenSet<string> WaterStrengthIncreaseIds = ImmutableHelper.CreateFrozenSet([WeatherIncreaseStrBad, WeatherIncreaseStrFresh]);

    public static readonly FrozenSet<string> PrewarningIds = ImmutableHelper.CreateFrozenSet([WeatherForecast3Id, WeatherWarningExt3Id]);

    public static readonly FrozenSet<string> WeatherWarningExtUnlockIds = ImmutableHelper.CreateFrozenSet(["WeatherExt1", "WeatherExt2", ]);
    public static readonly FrozenSet<string> WeatherForecastUnlockIds = ImmutableHelper.CreateFrozenSet(["WeatherPred1", "WeatherPred2", ]);

}

public enum GameWeatherStage
{
    Temperate,
    Warning,
    Hazardous,    
}