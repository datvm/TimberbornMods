
namespace WeatherEditor.Services;

public class ModdableWeatherModService : IModdableWeatherModService
{
    static WeatherInfo GetWeatherInfo(IModdedTemperateWeather q) => new(q.Id, q.Spec.Display);
    static HazardousWeatherInfo GetHazardousWeatherInfo(IModdedHazardousWeather q) => new(q.WeatherId, q.Spec.Display, q);

    // Can't use concrete type because the assembly may not be available
    readonly object history;
    readonly object registry;

    public ModdableWeatherModService(IContainer container)
    {
        history = container.GetInstance<ModdableWeatherHistoryProvider>();
        registry = container.GetInstance<ModdableWeatherRegistry>();
    }

    List<ModdableWeatherCycle> Cycles => (List<ModdableWeatherCycle>)((ModdableWeatherHistoryProvider)history).Cycles;

    ModdableWeatherCycle CurrentCycle
    {
        get => Cycles[^1]; // Don't use history.CurrentCycle, it's cached and is not up-to-date
        set
        {
            Debug.Log("Changing current cycle: " + Cycles[^1]);
            Cycles[^1] = value;
            Debug.Log("New current cycle: " + Cycles[^1]);
        }
    }

    public int TemperateWeatherDuration
    {
        get => CurrentCycle.TemperateWeatherDuration;
        set
        {
            var lastCycle = CurrentCycle;
            CurrentCycle = lastCycle with { TemperateWeather = lastCycle.TemperateWeather with { Duration = value } };
        }
    }
    public string CurrentTemperateWeatherId
    {
        get => CurrentCycle.TemperateWeather.Id;
        set
        {
            var curr = CurrentCycle;
            CurrentCycle = curr with { TemperateWeather = curr.TemperateWeather with { Id = value, } };
        }
    }
    public string CurrentTemperateWeatherName => ((ModdableWeatherHistoryProvider)history).CurrentCycleDetails.TemperateWeather.Spec.Display;

    // Do not use Lambda or LINQ here because the compiler will generate a static field with the type that may be unavailable
    public ImmutableArray<WeatherInfo> TemperateWeathers
    {
        get
        {
            List<WeatherInfo> weathers = [];
            foreach (var weather in ((ModdableWeatherRegistry)registry).TemperateWeathers)
            {
                weathers.Add(GetWeatherInfo(weather));
            }

            return [.. weathers];
        }
    }

    public int HazardousWeatherDuration
    {
        get => CurrentCycle.HazardousWeatherDuration;
        set
        {
            var lastCycle = CurrentCycle;
            CurrentCycle = lastCycle with { HazardousWeather = lastCycle.HazardousWeather with { Duration = value } };
        }
    }

    // Do not use Lambda or LINQ here because the compiler will generate a static field with the type that may be unavailable
    public ImmutableArray<HazardousWeatherInfo> HazardousWeathers
    {
        get
        {
            List<HazardousWeatherInfo> weathers = [];
            foreach (var weather in ((ModdableWeatherRegistry)registry).HazardousWeathers)
            {
                weathers.Add(GetHazardousWeatherInfo(weather));
            }
            return [.. weathers];
        }
    }
    public IHazardousWeather CurrentCycleHazardousWeather
    {
        get => ((ModdableWeatherHistoryProvider)history).CurrentCycleDetails.HazardousWeather;
        set
        {
            var curr = CurrentCycle;
            CurrentCycle = curr with { HazardousWeather = curr.HazardousWeather with { Id = value.Id, } };
        }
    }

    static readonly FieldInfo nextCycleField = typeof(ModdableWeatherHistoryProvider).GetField("nextCycleWeather", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
    public ModdableWeatherNextCycleWeather NextCycle
    {
        get => ((ModdableWeatherHistoryProvider)history).NextCycleWeather;
        set => nextCycleField.SetValue(history, value);
    }

    public string NextTemperateWeatherId
    {
        get => NextCycle.TemperateWeatherId;
        set
        {
            NextCycle = NextCycle with { TemperateWeatherId = value, };
        }
    }
    public string NextTemperateWeatherName => ((ModdableWeatherHistoryProvider)history).NextCycleTemperateWeather.Spec.Display;

    public bool IsSingleWeather
    {
        get => NextCycle.SingleMode;
        set
        {
            NextCycle = NextCycle with { SingleMode = value, };
        }
    }

    public bool IsSingleWeatherTemperate
    {
        get => NextCycle.IsTemperate;
        set
        {
            NextCycle = NextCycle with { IsTemperate = value, };
        }
    }

}
