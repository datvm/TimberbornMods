
namespace ModdableWeathers.Weathers;

public interface IModdableWeather
{
    string Id { get; }
    ModdableWeatherSpec Spec { get; }

    bool IsBenign => this is IModdableBenignWeather;
    bool IsHazardous => this is IModdableHazardousWeather;

    bool Active { get; }
    bool Enabled { get; }

    event WeatherChangedEventHandler? WeatherChanged;

    void Start(DetailedWeatherCycle cycle, DetailedWeatherCycleStage stage, bool onLoad);
    void End();

    int GetChance(WeatherCycleStageDecision stageDecision, WeatherCycleDecision cycleDecision, WeatherHistoryService history);
    int GetDuration(WeatherCycleStageDecision stageDecision, WeatherCycleDecision cycleDecision, WeatherHistoryService history);    
}

public interface IModdableBenignWeather : IModdableWeather { }

public interface IModdableHazardousWeather : IModdableWeather, IHazardousWeather
{

    // Should not be needed anymore
    int IHazardousWeather.GetDurationAtCycle(int cycle) => throw new NotImplementedException();

}