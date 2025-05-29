namespace ModdableWeather.Defaults;

public abstract class DefaultModdedWeather : IModdedWeather
{
    public abstract string Id { get; }
    public ModdedWeatherSpec Spec { get; set; } = null!;

    public event EventHandler? OnWeatherStarted;
    public event EventHandler? OnWeatherEnded;
    public event EventHandler? OnWeatherActiveChanged;

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
            if (counter < parameters.HandicapCycles)
            {
                var initHandicap = parameters.HandicapPerc;
                var deltaPerCycle = (100 - parameters.HandicapPerc) / parameters.HandicapCycles;

                var handicap = initHandicap + (deltaPerCycle * counter);
                minDay = minDay * handicap / 100;
                maxDay = maxDay * handicap / 100;
            }
        }

        if (minDay < 1) { minDay = 1; }
        if (minDay >= maxDay)
        {
            return Mathf.Max(1, maxDay);
        }

        return Random.RandomRangeInt(minDay, maxDay + 1);
    }

    public virtual int GetChance(int cycle, ModdableWeatherHistoryProvider history)
    {
        return cycle < Parameters.StartCycle ? 0 : Parameters.Chance;
    }

    public virtual void Start()
    {
        Active = true;
        OnWeatherStarted?.Invoke(this, EventArgs.Empty);
        OnWeatherActiveChanged?.Invoke(this, EventArgs.Empty);
    }

    public virtual void End()
    {
        Active = false;
        OnWeatherEnded?.Invoke(this, EventArgs.Empty);
        OnWeatherActiveChanged?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>
    /// This is for the game interface that will not be called anymore
    /// </summary>
    public int GetDurationAtCycle(int cycle) => throw new NotImplementedException();

    public override string ToString() => $"{Spec.Display} ({Id} - {GetType().FullName})";

}

public abstract class DefaultModdedWeather<TSettings>(TSettings settings) : DefaultModdedWeather
    where TSettings : DefaultWeatherSettings
{

    protected TSettings Settings { get; } = settings;
    public override WeatherParameters Parameters => Settings.Parameters;

}