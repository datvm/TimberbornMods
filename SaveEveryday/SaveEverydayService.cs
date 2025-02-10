using Timberborn.GameSaveRepositorySystem;
using Timberborn.GameSaveRuntimeSystem;
using Timberborn.SettlementNameSystem;
using Timberborn.TickSystem;
using Timberborn.TimeSystem;

namespace SaveEveryday;

public class SaveEverydayService(
    ModSettings settings,
    ISingletonLoader singletonLoader,
    IDayNightCycle time,
    SettlementNameService settlementNameService,
    GameSaver gameSaver,
    GameSaveRepository gameSaveRepository
) : ILoadableSingleton, ISaveableSingleton, ITickableSingleton
{
    const string NamePostfix = ".saveeveryday";
    const string NameFormat = "SaveDay{0}" + NamePostfix;

    static readonly SingletonKey SaveEverydayKey = new(nameof(SaveEveryday));
    static readonly PropertyKey<int> LastDayKey = new("LastAutosaveDay");

    public int LastAutoSaveDay { get; private set; }

    public void Load()
    {
        if (singletonLoader.HasSingleton(SaveEverydayKey))
        {
            var set = singletonLoader.GetSingleton(SaveEverydayKey);
            LastAutoSaveDay = set.Has(LastDayKey)
                ? set.Get(LastDayKey) : 0;
        }
    }

    public void Save(ISingletonSaver singletonSaver)
    {
        singletonSaver.GetSingleton(SaveEverydayKey).Set(LastDayKey, LastAutoSaveDay);
    }

    public void Tick()
    {
        if (!settings.Enabled) { return; }

        var day = time.DayNumber;
        if (day - LastAutoSaveDay >= settings.SaveFrequency)
        {
            LastAutoSaveDay = day;
            SaveGame(day);
        }
    }

    void SaveGame(int day)
    {
        string settlementName = settlementNameService.SettlementName;
        string saveName = string.Format(NameFormat, day);
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

            Debug.Log($"Deleted {exceedingSaves.Length} autosaves");
        }
        catch (GameSaverException ex)
        {
            Debug.LogError($"Error occured while deleting the oldest autosave: {ex.InnerException}");
        }
    }

}
