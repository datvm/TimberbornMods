namespace ModdableTimberborn.GameModes;

public class PersistentGameModeService(
    GameModeSpecService gameModeSpecService,
    ISingletonLoader loader,
    ISceneLoader sceneLoader,

    // Services for guessing
    NeedModificationService needModificationService,
    TemperateWeatherDurationService temperateWeatherDurationService,
    DroughtWeather droughtWeather,
    BadtideWeather badtideWeather,
    EffectProbabilityService effectProbabilityService,
    GoodRecoveryRateService goodRecoveryRateService
) : ILoadableSingleton, ISaveableSingleton
{
    static readonly SingletonKey SaveKey = new(nameof(PersistentGameModeService));
    static readonly PropertyKey<string> StartedModeKey = new("StartedMode");

    public static readonly FrozenSet<string> UnrecoverableProperties = [
        nameof(GameModeSpec.StartingAdults),
        nameof(GameModeSpec.AdultAgeProgress),
        nameof(GameModeSpec.StartingChildren),
        nameof(GameModeSpec.ChildAgeProgress),
        nameof(GameModeSpec.StartingFood),
        nameof(GameModeSpec.StartingWater),
    ];

    static readonly FrozenSet<PropertyInfo> RecoverableProperties = typeof(GameModeSpec)
        .GetProperties(BindingFlags.Public | BindingFlags.Instance)
        .Where(p => !UnrecoverableProperties.Contains(p.Name))
        .ToFrozenSet();

    public GameModeSpec Default => gameModeSpecService.GetDefaultSpec();

    public GameModeSpec? StartedMode { get; private set; }
    public bool StartedModeModified { get; private set; }

    public GameModeSpec ReconstructedMode { get; private set; } = null!;
    public GameModeSpec BestMatchedMode { get; private set; } = null!;
    public float MatchScore { get; private set; } = -1;

    public void Load()
    {
        if (!TryLoadFromNewGame())
        {
            TryLoadStartedMode();
            ReconstructCurrentMode();
        }

        TryMatchMode();
        CheckForModifiedStart();
    }

    bool TryLoadFromNewGame()
    {
        if (!sceneLoader.TryGetSceneParameters<GameSceneParameters>(out var sceneParams) || !sceneParams.NewGame)
        {
            return false;
        }

        var gameMode = sceneParams.NewGameConfiguration.GameMode;
        StartedMode = ReconstructedMode = gameMode;

        return true;
    }

    void TryLoadStartedMode()
    {
        if (!loader.TryGetSingleton(SaveKey, out var s)) { return; }

        if (s.Has(StartedModeKey))
        {
            var json = s.Get(StartedModeKey);
            StartedMode = JsonConvert.DeserializeObject<GameModeSpec>(json);
        }
    }

    void ReconstructCurrentMode()
    {
        var fallbackValue = StartedMode ?? Default;

        ReconstructedMode = new()
        {
            StartingAdults = fallbackValue.StartingAdults,
            AdultAgeProgress = fallbackValue.AdultAgeProgress,
            StartingChildren = fallbackValue.StartingChildren,
            ChildAgeProgress = fallbackValue.ChildAgeProgress,
            FoodConsumption = needModificationService._foodConsumption,
            WaterConsumption = needModificationService._waterConsumption,
            StartingFood = fallbackValue.StartingFood,
            StartingWater = fallbackValue.StartingWater,
            TemperateWeatherDuration = new()
            {
                Min = temperateWeatherDurationService._minTemperateWeatherDuration,
                Max = temperateWeatherDurationService._maxTemperateWeatherDuration,
            },
            DroughtDuration = new()
            {
                Min = droughtWeather._minDroughtDuration,
                Max = droughtWeather._maxDroughtDuration,
            },
            DroughtDurationHandicapMultiplier = droughtWeather._handicapMultiplier,
            DroughtDurationHandicapCycles = droughtWeather._handicapCycles,
            CyclesBeforeRandomizingBadtide = badtideWeather._cyclesBeforeRandomizingBadtideWeather,
            ChanceForBadtide = badtideWeather.ChanceForBadtide,
            BadtideDuration = new()
            {
                Min = badtideWeather._minDuration,
                Max = badtideWeather._maxDuration,
            },
            BadtideDurationHandicapMultiplier = badtideWeather._handicapMultiplier,
            BadtideDurationHandicapCycles = badtideWeather._handicapCycles,
            InjuryChance = effectProbabilityService._injuryChanceModifier,
            DemolishableRecoveryRate = goodRecoveryRateService.DemolishableRecoveryRate,
        };
    }

    void TryMatchMode()
    {
        var specs = gameModeSpecService.GetSpecsOrdered();
        
        int highScore = -1, highScoreIndex = -1;
        for (int i = 0; i < specs.Length; i++)
        {
            var spec = specs[i];

            var score = 0;
            foreach (var prop in RecoverableProperties)
            {
                var specValue = prop.GetValue(spec);
                var reconstructedValue = prop.GetValue(ReconstructedMode);
                if (Equals(specValue, reconstructedValue))
                {
                    ++score;
                }
            }

            if (score > highScore)
            {
                highScore = score;
                highScoreIndex = i;
            }
        }

        BestMatchedMode = highScore > 0 ? specs[highScoreIndex] : Default;
        MatchScore = highScore > 0 ? (float)highScore / RecoverableProperties.Count : 0f;
    }

    void CheckForModifiedStart()
    {
        if (StartedMode is null) 
        { 
            StartedModeModified = false; 
            return; 
        }

        // Compare with reconstructed mode
        foreach (var prop in RecoverableProperties)
        {
            var startedValue = prop.GetValue(StartedMode);
            var reconstructedValue = prop.GetValue(ReconstructedMode);
            if (!Equals(startedValue, reconstructedValue))
            {
                StartedModeModified = true;
                return;
            }
        }
    }

    public void Save(ISingletonSaver singletonSaver)
    {
        if (StartedMode is null) { return; }

        var s = singletonSaver.GetSingleton(SaveKey);
        s.Set(StartedModeKey, JsonConvert.SerializeObject(StartedMode));
    }

}
