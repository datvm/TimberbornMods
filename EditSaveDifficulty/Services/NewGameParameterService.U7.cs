namespace EditSaveDifficulty.Services;

public class NewGameParameterService(
    NeedModificationService needMods,
    TemperateWeatherDurationService temperWeather,
    DroughtWeather droughtWeather,
    BadtideWeather badtideWeather,
    EffectProbabilityService effectProb,
    GoodRecoveryRateService goodRec
)
{

    public NewGameMode GatherCurrentParameters() => new(
        0, default, 0, default,
        needMods._foodConsumption, needMods._waterConsumption,
        0, 0,
        new(temperWeather._minTemperateWeatherDuration, temperWeather._maxTemperateWeatherDuration),
        new(droughtWeather._minDroughtDuration, droughtWeather._maxDroughtDuration),
        droughtWeather._handicapMultiplier, droughtWeather._handicapCycles,
        badtideWeather._cyclesBeforeRandomizingBadtideWeather,
        badtideWeather.ChanceForBadtide, new(badtideWeather._minDuration, badtideWeather._maxDuration),
        badtideWeather._handicapMultiplier, badtideWeather._handicapCycles,
        effectProb._injuryChanceModifier,
        goodRec.DemolishableRecoveryRate
    );

    public void SetNewParameters(NewGameMode parameters)
    {
        needMods._foodConsumption = parameters.FoodConsumption;
        needMods._waterConsumption = parameters.WaterConsumption;
        temperWeather._minTemperateWeatherDuration = parameters.TemperateWeatherDuration.Min;
        temperWeather._maxTemperateWeatherDuration = parameters.TemperateWeatherDuration.Max;
        droughtWeather._minDroughtDuration = parameters.DroughtDuration.Min;
        droughtWeather._maxDroughtDuration = parameters.DroughtDuration.Max;
        droughtWeather._handicapMultiplier = parameters.DroughtDurationHandicapMultiplier;
        droughtWeather._handicapCycles = parameters.DroughtDurationHandicapCycles;
        badtideWeather._cyclesBeforeRandomizingBadtideWeather = parameters.CyclesBeforeRandomizingBadtide;
        badtideWeather.ChanceForBadtide = parameters.ChanceForBadtide;
        badtideWeather._minDuration = parameters.BadtideDuration.Min;
        badtideWeather._maxDuration = parameters.BadtideDuration.Max;
        badtideWeather._handicapMultiplier = parameters.BadtideDurationHandicapMultiplier;
        badtideWeather._handicapCycles = parameters.BadtideDurationHandicapCycles;
        effectProb._injuryChanceModifier = parameters.InjuryChance;
        goodRec.DemolishableRecoveryRate = parameters.DemolishableRecoveryRate;
    }

}
