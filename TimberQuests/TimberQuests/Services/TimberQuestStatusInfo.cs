namespace TimberQuests;

public class TimberQuestStatusInfo(TimberQuestSpec quest)
{

    public TimberQuestSpec Quest { get; } = quest;
    readonly TimberQuestStatus[] statuses = new TimberQuestStatus[quest.Steps.Length + 1];

    public TimberQuestStatus QuestStatus => statuses[0];
    public TimberQuestStatus GetStepStatus(int index) => statuses[index + 1];
    public ImmutableArray<TimberQuestStatus> GetStepsStatuses => [..statuses.Skip(1)];

    public event EventHandler<TimberQuestStatusChanged>? QuestStatusChanged;
    public event EventHandler<TimberQuestStepStatusChanged>? StepStatusChanged;

    internal TimberQuestStatusChanged? SetQuestStatus(TimberQuestStatus status)
    {
        var prev = statuses[0];
        if (prev == status) { return null; }

        statuses[0] = status;
        TimberQuestsUtils.Log(() => $"{Quest} status changed from {prev} to {status}");

        TimberQuestStatusChanged args = new(this, prev, status);
        QuestStatusChanged?.Invoke(this, args);
        return args;
    }

    internal TimberQuestStepStatusChanged? SetQuestStepStatus(int index, TimberQuestStatus status)
    {
        var prev = statuses[index + 1];
        if (prev == status) { return null; }

        statuses[index + 1] = status;
        TimberQuestsUtils.Log(() => $"{Quest} step {index} ({Quest.Steps[index].Name}) status changed from {prev} to {status}");

        TimberQuestStepStatusChanged args = new(this, index, prev, status);
        StepStatusChanged?.Invoke(this, args);
        return args;
    }

    public string Serialize() =>
        $"{Quest.Id};{string.Join(';', statuses.Select(s => (int)s))}";

    public static TimberQuestStatusInfo? TryDeserialize(string data, Func<string, TimberQuestSpec?> specFunc)
    {
        var parts = data.Split(';');

        var id = parts[0];
        var spec = specFunc(id);
        if (spec is null)
        {
            Debug.LogWarning($"Quest Id {id} no longer exist. Removing this quest data.");
            return null;
        }

        var status = new TimberQuestStatusInfo(spec);
        if (spec.Steps.Length != parts.Length - 1)
        {
            Debug.LogWarning($"{spec} has different number of steps now ({spec.Steps.Length} now vs expected {parts.Length - 1}).");
        }

        var copyingMax = Math.Min(status.statuses.Length, parts.Length - 1);
        for (var i = 0; i < copyingMax; i++)
        {
            var value = (TimberQuestStatus)int.Parse(parts[i + 1]);
            status.statuses[i] = value;
        }

        return status;
    }

}

