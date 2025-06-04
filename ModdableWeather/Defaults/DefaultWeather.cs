namespace ModdableWeather.Defaults;

public abstract class DefaultModdedWeather(
    ModdableWeatherSpecService moddableWeatherSpecService
) : IModdedWeather, ILoadableSingleton
{

    public abstract string Id { get; }
    public ModdedWeatherSpec Spec { get; protected set; } = null!;
    public bool Enabled => Parameters.Enabled;

    public event WeatherChangedEventHandler? OnWeatherActiveChanged;

    public bool Active { get; protected set; }

    public abstract WeatherParameters Parameters { get; }

    public virtual int GetDurationAtCycle(int cycle, ModdableWeatherHistoryProvider history)
    {
        var parameters = Parameters;

        var minDay = parameters.MinDay;
        var maxDay = parameters.MaxDay;

        if (parameters.HandicapCycles > 0)
        {
            var counter = history.GetWeatherCycleCount(Id);
            var handicap = ModdableWeatherUtils.CalculateHandicap(counter, parameters.HandicapCycles, () => parameters.HandicapPerc);

            if (handicap != 1f)
            {
                minDay = Mathf.RoundToInt(minDay * handicap);
                maxDay = Mathf.RoundToInt(maxDay * handicap);
            }
        }

        if (minDay < 1) { minDay = 1; }
        ModdableWeatherUtils.Log(() => $"Weather {ToString()} at cycle {cycle} has minDay: {minDay}, maxDay: {maxDay}.");

        var result = minDay >= maxDay ? Mathf.Max(1, maxDay) : Random.RandomRangeInt(minDay, maxDay + 1);
        ModdableWeatherUtils.Log(() => $"  Hit: {result}.");
        return result;
    }

    public virtual int GetChance(int cycle, ModdableWeatherHistoryProvider history)
    {
        return (!Enabled || cycle < Parameters.StartCycle) ? 0 : Parameters.Chance;
    }

    public virtual void Start(bool onLoad)
    {
        Active = true;
        OnWeatherActiveChanged?.Invoke(this, true, onLoad);
    }

    public virtual void End()
    {
        Active = false;
        OnWeatherActiveChanged?.Invoke(this, false, false);
    }

    /// <summary>
    /// This is for the game interface that will not be called anymore
    /// </summary>
    public int GetDurationAtCycle(int cycle) => throw new NotImplementedException();

    public override string ToString() => $"{Spec.Display} ({Id} - {GetType().FullName})";

    public virtual void Load()
    {
        Spec = moddableWeatherSpecService.Specs[Id];
    }

}

public abstract class DefaultModdedWeather<TSettings>(TSettings settings, ModdableWeatherSpecService moddableWeatherSpecService) : DefaultModdedWeather(moddableWeatherSpecService)
    where TSettings : DefaultWeatherSettings
{

    protected TSettings Settings { get; } = settings;
    public override WeatherParameters Parameters => Settings.Parameters;

}