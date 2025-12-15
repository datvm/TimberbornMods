namespace ModdableWeathers.Helpers;

public static class ModdableWeathersUtils
{

    public static T InstanceOrThrow<T>(this T? instance) where T : class
        => instance ?? throw new InvalidOperationException($"{typeof(T).Name} is not loaded yet!");

    public static T InstanceOrThrow<T>(this T? instance) where T : struct
        => instance ?? throw new InvalidOperationException($"{typeof(T).Name} is not loaded yet!");

    public static void LogVerbose(Func<string> message)
        => TimberUiUtils.LogVerbose(() => $"{nameof(ModdableWeathers)}: " + message());

    public static float CalculateHandicap(int counter, int handicapCycles, Func<int> getInitHandicapPercent)
    {
        if (handicapCycles == 0 || counter >= handicapCycles) { return 1f; }

        var initHandicap = getInitHandicapPercent();
        var deltaPerCycle = (100 - initHandicap) / handicapCycles;

        var handicap = initHandicap + (deltaPerCycle * counter);
        return handicap / 100f;
    }

    public static bool IsDrought(this IHazardousWeather weather) => weather is GameDroughtWeather;
    public static bool IsBadtide(this IHazardousWeather weather) => weather is GameBadtideWeather;

}
