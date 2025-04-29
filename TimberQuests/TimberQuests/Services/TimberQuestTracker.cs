namespace TimberQuests;

public class TimberQuestTracker(
    TimberQuestRegistry registry,
    ISingletonLoader loader,
    EventBus eb
) : ISaveableSingleton, ILoadableSingleton
{
    static readonly ListKey<string> QuestStatusKey = new("QuestStatus");

    readonly Dictionary<string, TimberQuestStatusInfo> questStatusInfo = [];

    public bool TryGet(string id, [MaybeNullWhen(false)] out TimberQuestStatusInfo statusInfo)
        => questStatusInfo.TryGetValue(id, out statusInfo);

    public void SetQuestStatus(string id, TimberQuestStatus status)
    {
        if (!TryGet(id, out var statusInfo))
        {
            var spec = registry.Get(id);
            statusInfo = questStatusInfo[id] = new(spec);
        }

        var args = statusInfo.SetQuestStatus(status);
        if (args is not null) { eb.Post(args.Value); }
    }

    public void SetQuestStepStatus(string id, int index, TimberQuestStatus status)
    {
        if (!TryGet(id, out var statusInfo))
        {
            throw new InvalidOperationException($"You must initialize the quest first by calling {nameof(SetQuestStatus)}.");
        }

        if (statusInfo.QuestStatus == TimberQuestStatus.Pending)
        {
            throw new InvalidOperationException($"You must set the quest status out of {TimberQuestStatus.Pending} before setting the step status.");
        }

        var args = statusInfo.SetQuestStepStatus(index, status);
        if (args is not null) { eb.Post(args.Value); }
    }

    public void Load()
    {
        LoadSavedData();
    }

    void LoadSavedData()
    {
        if (!loader.TryGetSingleton(TimberQuestsUtils.SaveKey, out var s)) { return; }

        if (s.Has(QuestStatusKey))
        {
            questStatusInfo.Clear();
            var statuses = s.Get(QuestStatusKey);

            foreach (var statusStr in statuses)
            {
                var status = TimberQuestStatusInfo.TryDeserialize(statusStr, registry.GetOrDefault);
                if (status != null)
                {
                    questStatusInfo[status.Quest.Id] = status;
                }
            }
        }
    }

    public void Save(ISingletonSaver singletonSaver)
    {
        var s = singletonSaver.GetSingleton(TimberQuestsUtils.SaveKey);

        s.Set(
            QuestStatusKey,
            [.. questStatusInfo.Values
                .Select(q => q.Serialize())]);
    }

}

