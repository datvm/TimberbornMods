#if TIMBER6
using Timberborn.GameSaveRepositorySystem;
using Timberborn.GameSaveRuntimeSystem;
using Timberborn.HazardousWeatherSystemUI;
using Timberborn.SettlementNameSystem;
using Timberborn.TimeSystem;

namespace SaveEveryday;

public class SaveEverydayService(
    ModSettings settings,
    ISingletonLoader singletonLoader,
    IDayNightCycle time,
    SettlementNameService settlementNameService,
    GameSaver gameSaver,
    GameSaveRepository gameSaveRepository,
    EventBus eb,
    HazardousWeatherApproachingTimer hazardTimer
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

    public void Load()
    {
        if (singletonLoader.HasSingleton(SaveEverydayKey))
        {
            var set = singletonLoader.GetSingleton(SaveEverydayKey);
            LastAutoSaveDay = set.Has(LastDayKey) ? set.Get(LastDayKey) : 0;
            LastAutoSaveWarningCycle = set.Has(LastWarningCycleKey) ? set.Get(LastWarningCycleKey) : 0;
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
    public void OnNextDay(DaytimeStartEvent _)
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
            var cycle = hazardTimer._weatherService.Cycle;

            if (LastAutoSaveWarningCycle != cycle &&
                hazardTimer.DaysToHazardousWeather <= HazardousWeatherApproachingTimer.ApproachingNotificationDays)
            {
                LastAutoSaveWarningCycle = cycle;
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
            gameSaver.QueueSaveSkippingNameValidation(saveReference, DeleteOldestExcessAutosaves);
        }
        catch (GameSaverException ex)
        {
            Debug.LogError($"Error occured while saving: {ex.InnerException}");
            gameSaveRepository.DeleteSaveSafely(saveReference);
        }
    }

    static bool IsAutosaveName(string name) => name.EndsWith(NamePostfix);

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