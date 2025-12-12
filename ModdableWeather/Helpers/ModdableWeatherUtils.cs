namespace ModdableWeather.Helpers;

public static class ModdableWeatherUtils
{

    public static void LogVerbose(Func<string> message) 
        => TimberUiUtils.LogVerbose(() => $"{nameof(ModdableWeather)}: " + message());

    public static float CalculateHandicap(int counter, int handicapCycles, Func<int> getInitHandicapPercent)
    {
        if (counter >= handicapCycles || handicapCycles == 0) { return 1f; }

        var initHandicap = getInitHandicapPercent();
        var deltaPerCycle = (100 - initHandicap) / handicapCycles;

        var handicap = initHandicap + (deltaPerCycle * counter);
        return handicap / 100f;
    }

}
