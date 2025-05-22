namespace ModdableWeather.Helpers;

public static class ModdableWeatherUtils
{

    internal static SingletonKey SaveKey = new(nameof(ModdableWeather));

    public static readonly bool HasMoreModLog = AppDomain.CurrentDomain.GetAssemblies().Any(q => q.GetName().Name == "MoreModLogs");

    public static void Log(Func<string> message)
    {
        if (HasMoreModLog)
        {
            Debug.Log($"{nameof(ModdableWeather)}: " + message());
        }
    }

}
