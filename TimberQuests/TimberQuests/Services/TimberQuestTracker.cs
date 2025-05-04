namespace TimberQuests;

public class TimberQuestTracker(
    TimberQuestRegistry registry,
    ISingletonLoader loader,
    EventBus eb
) : ISaveableSingleton, ILoadableSingleton
{
    static readonly ListKey<string> QuestStatusKey = new("QuestStatus");
    static readonly ListKey<string> SuccessfulQuestsKey = new("SuccessfulQuests");
    static readonly ListKey<string> FailedQuestsKey = new("FailedQuests");

    readonly Dictionary<string, TimberQuestStatusInfo> activeQuests = [];
    readonly HashSet<string> successfulQuests = [];
    readonly HashSet<string> failedQuests = [];

    public TimberQuestStatus GetStatus(string id)
    {
        if (successfulQuests.Contains(id))
        {
            return TimberQuestStatus.Completed;
        }
        else if (activeQuests.TryGetValue(id, out var statusInfo))
        {
            return statusInfo.QuestStatus;
        }
        else
        {
            return TimberQuestStatus.Pending;
        }
    }

    public bool TryGetActive(string id, [MaybeNullWhen(false)] out TimberQuestStatusInfo statusInfo)
        => activeQuests.TryGetValue(id, out statusInfo);

    public TimberQuestStatusInfo GetActive(string id)
        => TryGetActive(id, out var result) 
            ? result 
            : throw new InvalidOperationException($"Quest {id} is not active. You must call {nameof(StartQuest)} first.");

    public TimberQuestStatusInfo StartQuest(string id)
    {
        if (activeQuests.ContainsKey(id))
        {
            throw new InvalidOperationException($"Quest {id} is already active.");
        }

        successfulQuests.Remove(id);
        failedQuests.Remove(id);

        var spec = registry.Get(id);
        registry.InitializeQuestDetailsIfNeeded(spec);

        var statusInfo = activeQuests[id] = new(spec);
        SetQuestStatus(id, TimberQuestStatus.InProgress);

        return statusInfo;
    }

    public TimberQuestStatusInfo FinishQuest(string id, bool successful)
    {
        var statusInfo = GetActive(id);
        
        (successful ? successfulQuests : failedQuests).Add(id);

        activeQuests.Remove(id);
        SetQuestStatus(id, successful ? TimberQuestStatus.Completed : TimberQuestStatus.Failed);

        return statusInfo;
    }

    void SetQuestStatus(string id, TimberQuestStatus status)
    {
        var statusInfo = GetActive(id);

        var args = statusInfo.SetQuestStatus(status);
        if (args is not null) { eb.Post(args.Value); }
    }

    public void SetQuestStepStatus(string id, int index, TimberQuestStatus status)
    {
        var statusInfo = GetActive(id);

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
            activeQuests.Clear();
            var statuses = s.Get(QuestStatusKey);

            foreach (var statusStr in statuses)
            {
                var status = TimberQuestStatusInfo.TryDeserialize(statusStr, registry.GetOrDefault);
                if (status != null)
                {
                    activeQuests[status.Spec.Id] = status;
                }
            }
        }

        if (s.Has(SuccessfulQuestsKey))
        {
            successfulQuests.Clear();
            successfulQuests.AddRange(s.Get(SuccessfulQuestsKey));
        }

        if (s.Has(FailedQuestsKey))
        {
            failedQuests.Clear();
            failedQuests.AddRange(s.Get(FailedQuestsKey));
        }
    }

    public void Save(ISingletonSaver singletonSaver)
    {
        var s = singletonSaver.GetSingleton(TimberQuestsUtils.SaveKey);

        s.Set(
            QuestStatusKey,
            [.. activeQuests.Values
                .Select(q => q.Serialize())]);
        s.Set(SuccessfulQuestsKey, successfulQuests);
        s.Set(FailedQuestsKey, failedQuests);
    }

}

