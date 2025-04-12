#if TIMBER7
global using Timberborn.Autosaving;
global using Timberborn.HazardousWeatherSystemUI;
global using Timberborn.InputSystem;
global using Timberborn.GameSaveRepositorySystem;
global using Timberborn.GameSaveRuntimeSystem;
global using Timberborn.SettlementNameSystem;
global using Timberborn.TimeSystem;

namespace ConfigurableAutoSave;

public class ConfigurableAutoSaveService(
    Autosaver autosaver,
    MSettings s,
    InputService inputs,
    HazardousWeatherApproachingTimer hazardTimer,
    EventBus eb,
    SettlementNameService settlementNameService,
    GameSaver gameSaver,
    GameSaveRepository gameSaveRepository,
    ISingletonLoader loader
) : ILoadableSingleton, IInputProcessor, IUnloadableSingleton, ISaveableSingleton
{
    const string BadweatherSaveName = "weatherwarning.save";
    const string AutoSaveHotkeyId = "TriggerAutosave";

    static readonly SingletonKey SaveEverydayKey = new("ConfigurableAutoSaveService");
    static readonly PropertyKey<int> LastWarningCycleKey = new("LastAutosaveWarningCycle");

    AutosaverSpec? defaultSpec;

    public int LastAutoSaveWarningCycle { get; private set; }

    public void Load()
    {
        defaultSpec ??= autosaver._autosaverSpec;

        LoadData();

        s.OnSettingsChanged += UpdateSettings;
        UpdateSettings();

        eb.Register(this);
        inputs.AddInputProcessor(this);
    }

    void LoadData()
    {
        if (loader.TryGetSingleton(SaveEverydayKey, out var s))
        {
            LastAutoSaveWarningCycle = s.Has(LastWarningCycleKey) ? s.Get(LastWarningCycleKey) : 0;
        }
    }

    public bool ProcessInput()
    {
        if (inputs.IsKeyDown(AutoSaveHotkeyId))
        {
            autosaver._nextSaveTime = 0;
            return true;
        }

        return false;
    }

    public void Unload()
    {
        s.OnSettingsChanged -= UpdateSettings;
        inputs.RemoveInputProcessor(this);
    }

    private void UpdateSettings()
    {
        if (s.Enabled)
        {
            autosaver._autosaverSpec = new()
            {
                AutosavesPerSettlement = s.SaveCount,
                FrequencyInMinutes = s.SaveFrequency,
            };
        }
        else
        {
            autosaver._autosaverSpec = defaultSpec!;
        }

        autosaver.ScheduleNextSave();
    }


    [OnEvent]
    public void OnNextDay(DaytimeStartEvent _)
    {
        if (s.SaveWeatherWarning)
        {
            var cycle = hazardTimer._gameCycleService.Cycle;

            if (LastAutoSaveWarningCycle != cycle &&
                hazardTimer.DaysToHazardousWeather <= HazardousWeatherApproachingTimer.ApproachingNotificationDays)
            {
                LastAutoSaveWarningCycle = cycle;
                SaveBadweatherDay();
            }
        }
    }

    public void Save(ISingletonSaver singletonSaver)
    {
        var s = singletonSaver.GetSingleton(SaveEverydayKey);
        s.Set(LastWarningCycleKey, LastAutoSaveWarningCycle);
    }

    void SaveBadweatherDay()
    {
        string settlementName = settlementNameService.SettlementName;
        string saveName = BadweatherSaveName;

        var saveReference = new SaveReference(settlementName, saveName);
        try
        {
            gameSaver.QueueSaveSkippingNameValidation(saveReference, delegate { });
        }
        catch (GameSaverException ex)
        {
            Debug.LogError($"Error occured while saving: {ex.InnerException}");
            gameSaveRepository.DeleteSaveSafely(saveReference);
        }
    }

}
#endif