namespace ModdableWeather.Services;

public class ModdableWeatherGenerator(
    ModdableWeatherRegistry registry
)
{

    public CycleWeatherPair DecideForCycle(int cycle, ModdableWeatherService service)
    {
        return new(
            DecideForCycle(cycle, service, registry.TemperateWeathers, null),
            DecideForCycle(cycle, service, registry.HazardousWeathers, registry.NoneHazardousWeather)
        );
    }

    static T DecideForCycle<T>(int cycle, ModdableWeatherService service, IEnumerable<T> weathers, T? fallback)
        where T : IModdedWeather
    {
        var max = 0;
        List<(T, int)> values = [];

        ModdableWeatherUtils.Log(() => $"Deciding {typeof(T).Name} weather for cycle {cycle}");

        foreach (var w in weathers)
        {
            var chance = w.GetChance(cycle, service);
            if (chance <= 0) { continue; }

            max += chance;
            values.Add((w, chance));
            ModdableWeatherUtils.Log(() => $"  {w.Id}: {chance}");
        }

        if (values.Count == 0)
        {
            return fallback ??
                throw new InvalidOperationException($"No registered {typeof(T).Name} weather for cycle {cycle}.");
        }
        else if (values.Count == 1)
        {
            return values[0].Item1;
        }

        var hit = UnityEngine.Random.RandomRangeInt(0, max);
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
