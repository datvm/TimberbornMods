namespace ModdableWeathers.Common;

public class ModdableWeatherNotSupportedException(Type? replaceType, string? replaceName)
    : NotSupportedException(GetMessage(replaceType, replaceName))
{

    public ModdableWeatherNotSupportedException() : this(null, null) { }
    public ModdableWeatherNotSupportedException(Type? replaceType) : this(replaceType, null) { }

    static string GetMessage(Type? replaceType, string? replaceName)
    {
        var msg = $"{nameof(ModdableWeathers)} mod does not support this method.";

        if (replaceType is not null)
        {
            msg += $" Replacement: {replaceType.FullName}";

            if (replaceName is not null)
            {
                msg += $".{replaceName}";
            }
        }

        return msg;
    }

}

public class ModdableWeatherNotSupportedException<T>(string? replaceName)
    : ModdableWeatherNotSupportedException(typeof(T), replaceName)
{
    public ModdableWeatherNotSupportedException() : this(null) { }
}

public class WeatherDayServiceReplacedException(string? replaceName) : ModdableWeatherNotSupportedException<WeatherCycleService>(replaceName)
{

    public WeatherDayServiceReplacedException() : this(null) { }

}