namespace BeaverChronicles.Services.SpecNodes;

[MultiBind(typeof(ISpecNodeHandler))]
public class TimeLimitNodeHandler(
    ActiveChronicleEventService activeEvent,
    ILoc t,
    EventBus eb
) : ISpecNodeHandler
{
    public const string NodeType = "TimeLimit";
    public string ForType => NodeType;

    (ChronicleEventNodeSpec node, SpecChronicleEventController controller)? activeRef;
    bool restoreSubscriptionsInPostLoad;

    public void HandleNode(ChronicleEventNodeSpec node, SpecChronicleEventController controller)
    {
        if (controller.IsMini)
        {
            // Should not happen, caught by the ValidateSpec already
            throw new InvalidOperationException();
        }

        activeRef = (node, controller);

        var data = node.GetData<TimeLimitData>();

        if (GetDays(data, controller) is { } days)
        {
            activeEvent.RegisterTimeLimit(days, OnTimeLimitReached);
        }

        if (data.PanelTextLoc is { } textLoc)
        {
            activeEvent.SetActiveDescription(controller.FormatText(t.T(textLoc)));
        }

        if (data.Subscriptions.Length > 0)
        {
            SubscribeEvents(data.Subscriptions, controller);
        }

        if (data.Payments.Length > 0)
        {
            activeEvent.RegisterPayment(OnPaymentPaid, controller.FormatItems(data.Payments));
        }
    }

    void SubscribeEvents(ImmutableArray<TimeLimitSubscriptionData> subscriptions, SpecChronicleEventController controller)
    {
        var service = controller.HelperCollection.TimeLimitEvents;

        foreach (var s in subscriptions)
        {
            service.Subscribe(s.EventName, controller);
        }

        eb.Register(this);
    }

    void ClearEventSubscriptions(ImmutableArray<TimeLimitSubscriptionData> subscriptions, SpecChronicleEventController controller)
    {
        var service = controller.HelperCollection.TimeLimitEvents;

        foreach (var s in subscriptions)
        {
            service.Unsubscribe(s.EventName, controller);
        }

        eb.Unregister(this);
    }

    void OnPaymentPaid() => OnTimeLimitConcluded("Payment paid", d => d.GetData<TimeLimitData>().PaidNodeId);
    void OnTimeLimitReached() => OnTimeLimitConcluded("Time limit reached", d => d.NextNodeId);
    void OnEventTriggered(string? nodeId) => OnTimeLimitConcluded(null, _ => nodeId);

    void OnTimeLimitConcluded(string? evName, Func<ChronicleEventNodeSpec, string?> getNodeId)
    {
        activeEvent.Clear();

        if (activeRef is null)
        {
            // Should not happen
            return;
        }

        var (node, controller) = activeRef.Value;
        activeRef = null;

        ClearEventSubscriptions(node.GetData<TimeLimitData>().Subscriptions, controller);
        var nextNodeId = getNodeId(node);

        if (evName is not null)
        {
            node.LogVerbose(() => $"{evName}. Going to node: {nextNodeId}.");
        }

        controller.TriggerNode(nextNodeId);
    }

    public void RestoreGameState(ChronicleEventNodeSpec node, SpecChronicleEventController controller)
    {
        var data = node.GetData<TimeLimitData>();
        activeRef = (node, controller);

        if (GetDays(data, controller) is { } days)
        {
            activeEvent.RegisterSavedTimeLimit(days, OnTimeLimitReached);
        }

        if (activeEvent.HasSavedPayment)
        {
            activeEvent.RegisterSavedPayment(OnPaymentPaid);
        }

        if (data.Subscriptions.Length > 0)
        {
            restoreSubscriptionsInPostLoad = true;
        }
    }

    public void PostLoadGameState(ChronicleEventNodeSpec node, SpecChronicleEventController controller)
    {
        if (!restoreSubscriptionsInPostLoad)
        {
            return;
        }

        restoreSubscriptionsInPostLoad = false;

        if (activeRef is null || activeRef.Value.node != node || activeRef.Value.controller != controller)
        {
            return;
        }

        var data = node.GetData<TimeLimitData>();
        if (data.Subscriptions.Length > 0)
        {
            SubscribeEvents(data.Subscriptions, controller);
        }
    }

    [OnEvent]
    public void OnTimeLimitEvent(OnTimeLimitEvent e)
    {
        if (activeRef is null) { return; } // Should not happen

        var node = activeRef.Value.node;
        var data = node.GetData<TimeLimitData>();

        var nextNote = data.Subscriptions.FirstOrDefault(s => s.EventName == e.Name)?.NextNodeId;
        node.LogVerbose(() => $"Event triggered: {e.Name}, going to node: {nextNote}.");

        OnEventTriggered(nextNote);
    }

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
}
