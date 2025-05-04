namespace TimberQuests;

public class TimberQuestStatusInfo(TimberQuestSpec quest)
{
    public TimberQuestSpec Spec { get; } = quest;

    readonly TimberQuestStatus[] stepStatuses = new TimberQuestStatus[quest.Steps.Length];

    public TimberQuestStatus QuestStatus { get; private set; } = TimberQuestStatus.Pending;
    public ImmutableArray<TimberQuestStatus> StepsStatuses => [.. stepStatuses];
    public TimberQuestStatus GetStepStatus(int index) => stepStatuses[index];

    public event EventHandler<TimberQuestStatusChanged>? QuestStatusChanged;
    public event EventHandler<TimberQuestStepStatusChanged>? StepStatusChanged;

    internal TimberQuestStatusChanged? SetQuestStatus(TimberQuestStatus status)
    {
        var prev = QuestStatus;
        if (prev == status) { return null; }

        QuestStatus = status;
        TimberQuestsUtils.Log(() => $"{Spec} status changed from {prev} to {status}");

        TimberQuestStatusChanged args = new(this, prev, status);
        QuestStatusChanged?.Invoke(this, args);
        return args;
    }

    internal TimberQuestStepStatusChanged? SetQuestStepStatus(int index, TimberQuestStatus status)
    {
        var prev = stepStatuses[index];
        if (prev == status) { return null; }

        stepStatuses[index] = status;
        TimberQuestsUtils.Log(() => $"{Spec} step {index} ({Spec.Steps[index].Name}) status changed from {prev} to {status}");

        TimberQuestStepStatusChanged args = new(this, index, prev, status);
        StepStatusChanged?.Invoke(this, args);
        return args;
    }

    public string Serialize() =>
        $"{Spec.Id};{(int)QuestStatus};{string.Join(';', stepStatuses.Select(s => (int)s))}";

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

        if (parts.Length < 2)
        {
            Debug.LogWarning($"Quest Id {id} has invalid saved data. Removing this quest data.");
            return null;
        }

        var status = new TimberQuestStatusInfo(spec);
        // Expecting 1 part for QuestStatus and one for each step
        if (parts.Length != spec.Steps.Length + 2)
        {
            Debug.LogWarning($"{spec} has different number of steps now ({spec.Steps.Length} now vs expected {parts.Length - 2}).");
        }

        // Deserialize quest status
        status.QuestStatus = (TimberQuestStatus)int.Parse(parts[1]);

        // Deserialize step statuses
        var stepsCount = Math.Min(spec.Steps.Length, parts.Length - 2);
        for (var i = 0; i < stepsCount; i++)
        {
            var value = (TimberQuestStatus)int.Parse(parts[i + 2]);
            status.stepStatuses[i] = value;
        }

        return status;
    }
}

