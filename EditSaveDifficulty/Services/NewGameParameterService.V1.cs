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

    public GameModeSpec GatherCurrentParameters() => new()
    {
        StartingAdults = 0,
        AdultAgeProgress = new(),
        StartingChildren = 0,
        ChildAgeProgress = new(),
        FoodConsumption = needMods._foodConsumption,
        WaterConsumption = needMods._waterConsumption,
        StartingFood = 0,
        StartingWater = 0,
        TemperateWeatherDuration = new MinMaxSpec<int>
        {
            Min = temperWeather._minTemperateWeatherDuration,
            Max = temperWeather._maxTemperateWeatherDuration
        },
        DroughtDuration = new MinMaxSpec<int>
        {
            Min = droughtWeather._minDroughtDuration,
            Max = droughtWeather._maxDroughtDuration
        },
        DroughtDurationHandicapMultiplier = droughtWeather._handicapMultiplier,
        DroughtDurationHandicapCycles = droughtWeather._handicapCycles,
        CyclesBeforeRandomizingBadtide = badtideWeather._cyclesBeforeRandomizingBadtideWeather,
        ChanceForBadtide = badtideWeather.ChanceForBadtide,
        BadtideDuration = new MinMaxSpec<int>
        {
            Min = badtideWeather._minDuration,
            Max = badtideWeather._maxDuration
        },
        BadtideDurationHandicapMultiplier = badtideWeather._handicapMultiplier,
        BadtideDurationHandicapCycles = badtideWeather._handicapCycles,
        InjuryChance = effectProb._injuryChanceModifier,
        DemolishableRecoveryRate = goodRec.DemolishableRecoveryRate
    };

    public void SetNewParameters(GameModeSpec parameters)
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
