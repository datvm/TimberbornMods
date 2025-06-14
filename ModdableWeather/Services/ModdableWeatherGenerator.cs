namespace ModdableWeather.Services;

public class ModdableWeatherGenerator(
    ModdableWeatherRegistry registry,
    SingleWeatherModeSettings singleWeatherMode
)
{

    public ModdableWeatherCycle DecideForCycle(int cycle, ModdableWeatherHistoryProvider history)
    {
        var (temperate, hazard) = DecideWeatherForCycle(cycle, history);

        var next = history.NextCycleWeather;

        var tempDuration = !next.SingleMode || next.IsTemperate
            ? temperate.GetDurationAtCycle(cycle, history)
            : 0;
        var hazardDuration = !next.SingleMode || !next.IsTemperate
            ? hazard.GetDurationAtCycle(cycle, history)
            : 0;

        ModdableWeatherUtils.Log(() => $"""
            Decided weather for cycle {cycle}:
            - Temperate: {temperate.Id} ({tempDuration} days),
            - Hazardous: {hazard.WeatherId} ({hazardDuration} days)
            - Single Mode: {next.SingleMode} {(next.SingleMode ? (next.IsTemperate ? "Temperate" : "Hazardous") : "")}
        """);

        return new(
            cycle,
            new(temperate.Id, tempDuration),
            new(hazard.WeatherId, hazardDuration)
        );
    }

    public ModdableWeatherNextCycleWeather DecideNextCycleWeather(int currentCycle, ModdableWeatherHistoryProvider history)
    {
        var isSingleWeather = DecideSingleWeatherMode(currentCycle + 1);
        var isSingleWeatherTemperate = !isSingleWeather || DecideSingleWeatherTemperate();

        var nextTemperateWeather = isSingleWeatherTemperate
            ? DecideTemperateWeatherForCycle(currentCycle + 1, history)
            : registry.GameTemperateWeather;
        return new(isSingleWeather, isSingleWeatherTemperate, nextTemperateWeather);
    }

    public IModdedTemperateWeather DecideTemperateWeatherForCycle(int cycle, ModdableWeatherHistoryProvider history)
        => DecideForCycle(cycle, history, registry.TemperateWeathers, registry.GameTemperateWeather);

    bool DecideSingleWeatherMode(int cycle)
    {
        if (!singleWeatherMode.Enabled.Value
            || cycle < singleWeatherMode.StartCycle.Value) { return false; }

        var chance = singleWeatherMode.Chance.Value;
        ModdableWeatherUtils.Log(() => $"""
            Deciding Single Weather Mode for cycle {cycle}:
            - Chance: {chance}%,
            """);
        if (chance <= 0) { return false; }
        if (chance >= 100) { return true; }

        var hit = Random.RandomRangeInt(0, 100);
        ModdableWeatherUtils.Log(() => $"- Hit: {hit} ({hit < chance})");

        return hit < chance;
    }

    bool DecideSingleWeatherTemperate()
    {
        var chance = singleWeatherMode.TemperateChance.Value;
        if (chance <= 0) { return false; }
        if (chance >= 100) { return true; }

        ModdableWeatherUtils.Log(() => $"""
            Deciding Single Weather Temperate:
            - Chance: {chance}%,
            """);

        var hit = Random.RandomRangeInt(0, 100);
        ModdableWeatherUtils.Log(() => $"- Hit: {hit} ({hit < chance})");

        return hit < chance;
    }

    CycleWeatherPair DecideWeatherForCycle(int cycle, ModdableWeatherHistoryProvider history)
    {
        var next = history.HasNextCycleWeather ?
            history.NextCycleWeather :
            new(false, true, registry.GameTemperateWeather);

        var temperateWeather = next.TemperateWeather is null ?
            DecideTemperateWeatherForCycle(cycle, history) :
            next.TemperateWeather;

        var hazardousWeathers = next.SingleMode && next.IsTemperate
            ? registry.NoneHazardousWeather
            : DecideForCycle(cycle, history, registry.HazardousWeathers, registry.NoneHazardousWeather);

        return new(temperateWeather, hazardousWeathers);
    }

    T DecideForCycle<T>(int cycle, ModdableWeatherHistoryProvider history, IEnumerable<T> weathers, T fallback)
        where T : IModdedWeather
    {
        var max = 0;
        List<(T, int)> values = [];

        ModdableWeatherUtils.Log(() => $"Deciding {typeof(T).Name} weather for cycle {cycle}");

        foreach (var w in weathers)
        {
            if (!w.Enabled) { continue; }

            var chance = w.GetChance(cycle, history);
            if (chance <= 0) { continue; }

            max += chance;
            values.Add((w, chance));
            ModdableWeatherUtils.Log(() => $"  {w.Id}: {chance}");
        }

        if (values.Count == 0)
        {
            return fallback;
        }
        else if (values.Count == 1)
        {
            return values[0].Item1;
        }

        var hit = Random.RandomRangeInt(0, max);
        ModdableWeatherUtils.Log(() => $"  * Hit: {hit}");
        foreach (var weather in values)
        {
            hit -= weather.Item2;
            if (hit < 0)
            {
                ModdableWeatherUtils.Log(() => $"  * Selected: {weather.Item1.Id}");
                return weather.Item1;
            }
        }

        // Should not happen
        throw new InvalidDataException();
    }

}
