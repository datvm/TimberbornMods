namespace BeaverChronicles.Services.Helpers;

[BindSingleton]
public class FlagHelper(
    ChronicleEventFlagService persistence,
    MapNameService mapNameService,
    ISingletonLoader loader,
    ChronicleEventRecords history
) : ILoadableSingleton, ISaveableSingleton
{
    public const char FlagSeparator = '.';

    static readonly SingletonKey SaveKey = new(nameof(FlagHelper));
    static readonly PropertyKey<bool> Initialized = new("Initialized");

    public void Load()
    {
        if (loader.TryGetSingleton(SaveKey, out var s)
            && s.Has(Initialized) && s.Get(Initialized))
        {
            return;
        }

        MarkMapName();
        MarkBackwardCompatibilityFlags();
    }

    public void Save(ISingletonSaver singletonSaver)
    {
        var s = singletonSaver.GetSingleton(SaveKey);
        s.Set(Initialized, true);
    }

    public bool HasFlag(string flag) => persistence.HasFlag(flag);
    public bool HasFlags(IReadOnlyList<string> flags, ConditionType condition) => condition.Evaluate(flags, persistence.HasFlag);
    public bool HasAnyFlags(IReadOnlyList<string> flags) => flags.FastAny(persistence.HasFlag);
    public bool HasAllFlags(IReadOnlyList<string> flags) => flags.FastAll(persistence.HasFlag);
    public void AddFlag(string flag) => persistence.AddFlag(flag);
    public void RemoveFlag(string flag) => persistence.RemoveFlag(flag);

    public void MarkEvent(string eventId, int occurence)
    {
        persistence.AddFlag(GetEventFlag(eventId));
        persistence.AddFlag(GetEventOccurenceFlag(eventId, occurence));
    }

    public void MarkEventChoice(string eventId, int occurence, int choiceNo, int choiceIndexBase1)
    {
        persistence.AddFlag(GetEventChoiceFlag(eventId, occurence, choiceNo, choiceIndexBase1));
    }

    public void MarkEventFinished(string eventId)
    {
        persistence.AddFlag(GetFinishedEventFlag(eventId));
    }

    void MarkMapName() => persistence.AddFlag(GetMapFlag());

    void MarkBackwardCompatibilityFlags()
    {
        Dictionary<string, int> occurences = [];

        foreach (var e in history.Records)
        {
            var times = occurences.GetValueOrDefault(e.Id, 0) + 1;

            // First mark the event
            MarkEvent(e.Id, times);

            // Then mark the choices
            foreach (var (c, i) in e.GetRecordedChoices().Select((c, i) => (c, i)))
            {
                MarkEventChoice(e.Id, times, i + 1, c + 1);
            }

            occurences[e.Id] = times;
        }

        foreach (var id in history.FinishedEventIds)
        {
            MarkEventFinished(id);
        }
    }

    public string GetMapFlag() => GetMapFlag(mapNameService.Name ?? "N/A");
    public static string GetMapFlag(string mapName) => "MAP." + mapName;

    public static string GetEventFlag(string eventId) => $"EVENT.{eventId}";
    public static string GetEventOccurenceFlag(string eventId, int time) => $"EVENT.{eventId}.{time}"; // Always print out the occurence, in addition to the base event flag for boolean checks.
    public static string GetEventChoiceFlag(string eventId, int time, int choiceNo, int choiceIndexBase1) => $"EVENT.{eventId}{FirstOccurenceEmpty(time)}.C{choiceNo}.{choiceIndexBase1}";
    public static string GetEventStatusFlag(string eventId, int time, int choiceNo, bool successful) => $"EVENT.{eventId}{FirstOccurenceEmpty(time)}.C{choiceNo}.S{(successful ? "Y" : "N")}";
    static string FirstOccurenceEmpty(int time) => time == 1 ? "" : ("." + time);

    public static string GetFinishedEventFlag(string eventId) => $"EVENT.{eventId}.FINISHED";

}
