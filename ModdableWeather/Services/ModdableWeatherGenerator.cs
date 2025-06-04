namespace ModdableWeather.Services;

public class ModdableWeatherGenerator(
    ModdableWeatherRegistry registry
)
{

    public ModdableWeatherCycle DecideForCycle(int cycle, ModdableWeatherHistoryProvider history)
    {
        var (temperate, hazard) = DecideWeatherForCycle(cycle, history);

        var tempDuration = temperate.GetDurationAtCycle(cycle, history);
        var hazardDuration = hazard.GetDurationAtCycle(cycle, history);

        ModdableWeatherUtils.Log(() => 
            $"Decided weather for cycle {cycle}:" +
            $" {temperate.Id} ({tempDuration} days)," +
            $" {hazard.WeatherId} ({hazardDuration} days)");

        return new(
            cycle,
            new(temperate.Id, tempDuration),
            new(hazard.WeatherId, hazardDuration)
        );
    }

    public IModdedTemperateWeather DecideTemperateWeatherForCycle(int cycle, ModdableWeatherHistoryProvider history)
        => DecideForCycle(cycle, history, registry.TemperateWeathers, registry.GameTemperateWeather);

    CycleWeatherPair DecideWeatherForCycle(int cycle, ModdableWeatherHistoryProvider history)
    {
        var temperateWeather = history.HasNextCycleTemperateWeather ?
            history.NextCycleTemperateWeather :
            DecideTemperateWeatherForCycle(cycle, history);

        return new(
            temperateWeather,
            DecideForCycle(cycle, history, registry.HazardousWeathers, registry.NoneHazardousWeather)
        );
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
