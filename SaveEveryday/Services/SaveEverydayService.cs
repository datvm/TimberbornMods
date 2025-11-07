namespace SaveEveryday.Services;

public class SaveEverydayService(
    MSettings settings,
    ISingletonLoader singletonLoader,
    IDayNightCycle time,
    GameCycleService cycles,
    SettlementReferenceService settlementReferenceService,
    GameSaver gameSaver,
    GameSaveRepository gameSaveRepository,
    EventBus eb,
    HazardousWeatherApproachingTimer hazardTimer,
    QuickNotificationService notf,
    ILoc t
) : ILoadableSingleton, ISaveableSingleton
{
    const string NamePostfix = ".saveeveryday";
    const string NameFormat = "SaveDay{2}" + NamePostfix;
    const string BadweatherSaveName = "weatherwarning.save";
    const string FirstDaySaveName = "Cycle-{0}.save";

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
        var day = time.DayNumber;
        if (settings.Enabled)
        {
            if (day - LastAutoSaveDay >= settings.SaveFrequency)
            {
                LastAutoSaveDay = day;
                SaveGame(day);
            }
        }

        if (settings.SaveWeatherWarning)
        {
            var cycle = hazardTimer._gameCycleService.Cycle;

            if (LastAutoSaveWarningCycle != cycle &&
                hazardTimer.GetProgress() >= 0)
            {
                LastAutoSaveWarningCycle = hazardTimer._gameCycleService.Cycle;
                SaveGame(day, true);
            }
        }

        if (settings.SaveFirstDay.Value && cycles.CycleDay == 1)
        {
            SaveStartCycle();
        }
    }

    void SaveStartCycle()
    {
        var saveName = string.Format(FirstDaySaveName, cycles.Cycle);
        SaveGame(saveName);
    }

    void SaveGame(int day, bool badweather = false)
    {
        string saveName = GetSaveName(day, badweather);
        SaveGame(saveName);
    }

    void SaveGame(string saveName)
    {
        var saveReference = new SaveReference(saveName, settlementReferenceService.SettlementReference);
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

    string GetSaveName(int day, bool badweather)
    {
        if (badweather) { return BadweatherSaveName; }

        var nameFormat = settings.AutoSaveFilename;
        if (string.IsNullOrEmpty(nameFormat))
        {
            nameFormat = NameFormat;
        }

        return string.Format(nameFormat,
            cycles.Cycle,
            cycles.CycleDay,
            day);
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
            var saves = gameSaveRepository.GetSaves(settlementReferenceService.SettlementReference)
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