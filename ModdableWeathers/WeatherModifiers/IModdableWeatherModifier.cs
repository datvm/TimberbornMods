
namespace ModdableWeathers.WeatherModifiers;

public interface IModdableWeatherModifier
{
    string Id { get; }
    ModdableWeatherModifierSpec Spec { get; }
    bool Active { get; }
    ModdableWeatherModifierSettings Settings { get; }

    event WeatherModifierChangedEventHandler? WeatherModifierChanged;

    void Start(DetailedWeatherStageReference stage, WeatherHistoryService history, bool onLoad);
    void End();

    int GetChance(WeatherCycleStageDecision stageDecision, WeatherCycleDecision cycleDecision, WeatherHistoryService history);
    void ReloadSettings();
}

public interface IModdableWeatherModifier<TSettings> : IModdableWeatherModifier
    where TSettings : ModdableWeatherModifierSettings
{
    new TSettings Settings { get; }
    ModdableWeatherModifierSettings IModdableWeatherModifier.Settings => Settings;
}

public interface IModdableWeatherDecisionModifier : IModdableWeatherModifier
{
    void ModifyDecision(WeatherCycleStageDecision stageDecision, WeatherCycleDecision cycleDecision, WeatherHistoryService history);
}