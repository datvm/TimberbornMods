namespace BeaverChronicles.Services.SpecNodes;

[MultiBind(typeof(ISpecNodeHandler))]
public class TimeLimitNodeHandler(
    ActiveChronicleEventService activeEvent,
    ILoc t
) : ISpecNodeHandler, ITickableSingleton
{
    public const string NodeType = "TimeLimit";
    const string RemainingDaysParameter = "TimeLimitRemainingDays";
    public string ForType => NodeType;

    bool hasCustomTrigger;
    (ChronicleEventNodeSpec node, SpecChronicleEventController controller, TimeLimitData data)? activeRef;

    int lastCustomTriggerHour;
    int lastCustomTriggerDay;

    TimeLimitData SetActiveNode(ChronicleEventNodeSpec node, SpecChronicleEventController controller)
    {
        var data = node.GetData<TimeLimitData>();
        if (data.CustomTriggers.Length > 0)
        {
            hasCustomTrigger = true;
        }

        activeRef = (node, controller, data);

        lastCustomTriggerHour = GetCurrentHour(controller);
        lastCustomTriggerDay = GetCurrentDay(controller);

        return data;
    }

    void ClearActiveNode()
    {
        activeRef = null;
        hasCustomTrigger = false;
    }

    public void HandleNode(ChronicleEventNodeSpec node, SpecChronicleEventController controller)
    {
        if (controller.IsMini)
        {
            // Should not happen, caught by the ValidateSpec already
            throw new InvalidOperationException();
        }

        var data = SetActiveNode(node, controller);
        if (GetDays(data, controller) is { } days)
        {
            activeEvent.RegisterTimeLimit(days, OnTimeLimitReached);
        }

        if (data.PanelTextLoc is { } textLoc)
        {
            activeEvent.SetActiveDescription(controller.FormatText(t.T(textLoc)));
        }

        if (data.Payments.Length > 0)
        {
            activeEvent.RegisterPayment(OnPaymentPaid, controller.FormatItems(data.Payments));
        }
    }

    void OnPaymentPaid() => OnTimeLimitConcluded("Payment paid", d => d.GetData<TimeLimitData>().PaidNodeId);
    void OnTimeLimitReached() => OnTimeLimitConcluded("Time limit reached", d => d.NextNodeId);
    void OnCustomTrigger(TimeLimitCustomTriggerData trigger) => OnTimeLimitConcluded("Custom trigger matched", _ => trigger.TriggerNodeId);

    void OnTimeLimitConcluded(string? evName, Func<ChronicleEventNodeSpec, string?> getNodeId)
    {

        var remainingDays = activeEvent.RemainingDays;
        activeEvent.Clear();

        if (activeRef is null)
        {
            // Should not happen
            return;
        }

        var (node, controller, _) = activeRef.Value;
        ClearActiveNode();

        var nextNodeId = getNodeId(node);

        controller.CurrentRecord.CustomParameters[RemainingDaysParameter] = remainingDays.ToString("F2");

        if (evName is not null)
        {
            node.LogVerbose(() => $"{evName}. Going to node: {nextNodeId}.");
        }

        controller.TriggerNode(nextNodeId);
    }

    public void RestoreGameState(ChronicleEventNodeSpec node, SpecChronicleEventController controller)
    {
        var data = SetActiveNode(node, controller);

        if (GetDays(data, controller) is { } days)
        {
            activeEvent.RegisterSavedTimeLimit(days, OnTimeLimitReached);
        }

        if (activeEvent.HasSavedPayment)
        {
            activeEvent.RegisterSavedPayment(OnPaymentPaid);
        }
    }

    public void Tick()
    {
        if (!hasCustomTrigger || activeRef is null)
        {
            return;
        }

        var (node, controller, data) = activeRef.Value;
        if (data.CustomTriggers.Length == 0) // Should not happen
        {
            hasCustomTrigger = false;
            return;
        }

        var currentHour = GetCurrentHour(controller);
        var currentDay = GetCurrentDay(controller);

        foreach (var trigger in data.CustomTriggers)
        {
            if (!ShouldCheck(trigger, currentHour, currentDay))
            {
                continue;
            }

            if (controller.EvaluateConditionNode(trigger.ConditionNodeId))
            {
                node.LogVerbose(() => $"Custom trigger condition {trigger.ConditionNodeId} matched, going to node: {trigger.TriggerNodeId}.");
                OnCustomTrigger(trigger);
                break;
            }
        }

        lastCustomTriggerHour = currentHour;
        lastCustomTriggerDay = currentDay;
    }

    public void PostLoadGameState(ChronicleEventNodeSpec node, SpecChronicleEventController controller) { }

    static float? GetDays(TimeLimitData data, SpecChronicleEventController controller)
    {
        if (data.Days is null && data.Hours is null)
        {
            return null;
        }

        var result = data.Days is not null
            ? controller.FormatTextFloat(data.Days)
            : (controller.FormatTextFloat(data.Hours) / 24f);

        if (result <= 0)
        {
            throw new InvalidOperationException($"Event {controller.Event.Id}: Time limit must be greater than zero.");
        }

        return result;
    }

    bool ShouldCheck(TimeLimitCustomTriggerData trigger, int currentHour, int currentDay)
        => trigger.Interval switch
        {
            TimeLimitCustomTriggerInterval.Tick => true,
            TimeLimitCustomTriggerInterval.Hour => currentHour != lastCustomTriggerHour,
            TimeLimitCustomTriggerInterval.Day => currentDay != lastCustomTriggerDay,
            _ => throw new InvalidOperationException($"Unknown time limit custom trigger interval: {trigger.Interval}."),
        };

    static int GetCurrentHour(SpecChronicleEventController controller)
        => (int)controller.HelperCollection.GameStats.GetFloatStat(GameStats.TimeTodayHours);
    static int GetCurrentDay(SpecChronicleEventController controller)
        => controller.HelperCollection.GameStats.GameDayNumber;
}
