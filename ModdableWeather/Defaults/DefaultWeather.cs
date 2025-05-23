namespace ModdableWeather.Defaults;

public abstract class DefaultModdedWeather : IModdedWeather
{
    public abstract string Id { get; }
    public ModdedWeatherSpec Spec { get; set; } = null!;

    public abstract WeatherParameters Parameters { get; }

    public virtual int GetDurationAtCycle(int cycle, ModdableWeatherService service)
    {
        var parameters = Parameters;

        var minDay = parameters.MinDay;
        var maxDay = parameters.MaxDay;

        if (parameters.HandicapCycles > 0)
        {
            var counter = service.GetWeatherCycleCount(Id);
            if (counter < parameters.HandicapCycles)
            {
                var initHandicap = parameters.HandicapPerc;
                var deltaPerCycle = (100 - parameters.HandicapPerc) / parameters.HandicapCycles;

                var handicap = initHandicap + (deltaPerCycle * counter);
                minDay = minDay * handicap / 100;
                maxDay = maxDay * handicap / 100;
            }
        }

        return Random.RandomRangeInt(minDay, maxDay + 1);
    }

    public virtual int GetChance(int cycle, ModdableWeatherService service)
    {
        return cycle < Parameters.StartCycle ? 0 : Parameters.Chance;
    }
}

public abstract class DefaultModdedWeather<TSettings>(TSettings settings) : DefaultModdedWeather
    where TSettings : DefaultWeatherSettings
{

    protected TSettings Settings { get; } = settings;
    public override WeatherParameters Parameters => Settings.Parameters;

}