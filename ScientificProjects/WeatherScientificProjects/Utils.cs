global using ScientificProjects.Management;
global using Timberborn.WaterSourceSystem;

namespace WeatherScientificProjects;

public static class WeatherProjectsUtils
{

    public const string WeatherDecreaseStrBad = "WeatherDecreaseStrBad";
    public const string WeatherDecreaseStrFresh = "WeatherDecreaseStrFresh";
    public const string WeatherIncreaseStrBad = "WeatherIncreaseStrBad";
    public const string WeatherIncreaseStrFresh = "WeatherIncreaseStrFresh";

    public const string WeatherForecast3Id = "WeatherPred3";
    public const string WeatherWarningExt3Id = "WeatherExt3";


    public static readonly ImmutableHashSet<string> FreshWaterStrengthIds = [WeatherDecreaseStrFresh, WeatherIncreaseStrFresh];
    public static readonly ImmutableHashSet<string> BadWaterStrengthIds = [WeatherDecreaseStrBad, WeatherIncreaseStrBad];

    public static readonly ImmutableHashSet<string> PrewarningIds = [WeatherForecast3Id, WeatherWarningExt3Id];

    public static readonly ImmutableHashSet<string> WeatherWarningExtIds = ["WeatherExt1", "WeatherExt2", WeatherWarningExt3Id];
    public static readonly ImmutableHashSet<string> WeatherForecastIds = ["WeatherPred1", "WeatherPred2", WeatherForecast3Id];

}



