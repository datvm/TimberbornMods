#if TIMBER7
global using Timberborn.GameCycleSystem;
global using Timberborn.GameSaveRepositorySystem;
global using Timberborn.GameSaveRuntimeSystem;
global using Timberborn.HazardousWeatherSystemUI;
global using Timberborn.SettlementNameSystem;

namespace SaveEveryday.Services;

public class SaveEverydayService(
    ModSettings settings,
    ISingletonLoader singletonLoader,
    IDayNightCycle time,
    SettlementNameService settlementNameService,
    GameSaver gameSaver,
    GameSaveRepository gameSaveRepository,
    EventBus eb,
    HazardousWeatherApproachingTimer hazardTimer,
    QuickNotificationService notf,
    ILoc t
) : ILoadableSingleton, ISaveableSingleton
{
    const string NamePostfix = ".saveeveryday";
    const string NameFormat = "SaveDay{0}" + NamePostfix;
    const string BadweatherSaveName = "weatherwarning.save";

    static readonly SingletonKey SaveEverydayKey = new(nameof(SaveEveryday));
    static readonly PropertyKey<int> LastDayKey = new("LastAutosaveDay");
    static readonly PropertyKey<int> LastWarningCycleKey = new("LastAutosaveWarningCycle");

    public int LastAutoSaveDay { get; private set; }
    public int LastAutoSaveWarningCycle { get; private set; }

    public bool WillSaveNextDay
    {
        get
        {
            return time.DayNumber + 1 - LastAutoSaveDay >= settings.SaveFrequency;
        }
    }

    public void Load()
    {
        if (singletonLoader.TryGetSingleton(SaveEverydayKey, out var s))
        {
            LastAutoSaveDay = s.Has(LastDayKey) ? s.Get(LastDayKey) : 0;
            LastAutoSaveWarningCycle = s.Has(LastWarningCycleKey) ? s.Get(LastWarningCycleKey) : 0;
        }

        eb.Register(this);
    }

    public void Save(ISingletonSaver singletonSaver)
    {
        var s = singletonSaver.GetSingleton(SaveEverydayKey);
        s.Set(LastDayKey, LastAutoSaveDay);
        s.Set(LastWarningCycleKey, LastAutoSaveWarningCycle);
    }

    [OnEvent]
    public void OnNextDay(CycleDayStartedEvent _)
    {
        if (!settings.Enabled) { return; }

        var day = time.DayNumber;
        if (day - LastAutoSaveDay >= settings.SaveFrequency)
        {
            LastAutoSaveDay = day;
            SaveGame(day);
        }

        if (settings.SaveWeatherWarning)
        {
            var cycle = hazardTimer._gameCycleService.Cycle;

            if (LastAutoSaveWarningCycle != cycle &&
                hazardTimer.DaysToHazardousWeather <= HazardousWeatherApproachingTimer.ApproachingNotificationDays)
            {
                LastAutoSaveWarningCycle = hazardTimer._gameCycleService.Cycle;
                SaveGame(day, true);
            }
        }
    }

    void SaveGame(int day, bool badweather = false)
    {
        string settlementName = settlementNameService.SettlementName;
        string saveName = badweather ? BadweatherSaveName : string.Format(NameFormat, day);
        Debug.Log($"Saving game as {saveName}");

        var saveReference = new SaveReference(settlementName, saveName);
        try
        {
            gameSaver.QueueSaveSkippingNameValidation(saveReference, OnSaveDone);
        }
        catch (GameSaverException ex)
        {
            Debug.LogError($"Error occured while saving: {ex.InnerException}");
            gameSaveRepository.DeleteSaveSafely(saveReference);
        }
    }

    static bool IsAutosaveName(string name) => name.EndsWith(NamePostfix);

    void OnSaveDone()
    {
        notf.SendNotification(t.T("LV.SE.AutosaveDoneText"));
        DeleteOldestExcessAutosaves();
    }

    void DeleteOldestExcessAutosaves()
    {
        try
        {
            var saves = gameSaveRepository.GetSaves(settlementNameService.SettlementName)
                .Where(save => IsAutosaveName(save.SaveName));

            var exceedingSaves = saves
                .Skip(settings.SaveCount)
                .ToImmutableArray();
            foreach (var save in exceedingSaves)
            {
                gameSaveRepository.DeleteSave(save);
            }
        }
        catch (GameSaverException ex)
        {
            Debug.LogError($"Error occured while deleting the oldest autosave: {ex.InnerException}");
        }
    }

}
#endif