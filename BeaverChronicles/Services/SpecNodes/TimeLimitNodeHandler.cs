namespace BeaverChronicles.Services.SpecNodes;

[MultiBind(typeof(ISpecNodeHandler))]
public class TimeLimitNodeHandler(
    ActiveChronicleEventService activeEvent,
    ILoc t
) : ISpecNodeHandler
{
    public const string NodeType = "TimeLimit";
    public string ForType => NodeType;

    (ChronicleEventNodeSpec node, SpecChronicleEventController controller)? activeRef;

    public void HandleNode(ChronicleEventNodeSpec node, SpecChronicleEventController controller)
    {
        if (controller.IsMini)
        {
            // Should not happen, caught by the ValidateSpec already
            throw new InvalidOperationException();
        }

        activeRef = (node, controller);

        var data = node.GetData<TimeLimitData>();

        var days = GetDays(data, controller);
        activeEvent.RegisterTimeLimit(days, OnTimeLimitReached);

        if (data.PanelTextLoc is { } textLoc)
        {
            activeEvent.SetActiveDescription(controller.FormatText(t.T(textLoc)));
        }

        if (data.Payments.Length > 0)
        {
            activeEvent.RegisterPayment(OnPaymentPaid, controller.FormatItems(data.Payments));
        }
    }

    void OnPaymentPaid() => OnTimeLimitConcluded(d => d.GetData<TimeLimitData>().PaidNodeId);
    void OnTimeLimitReached() => OnTimeLimitConcluded(d => d.NextNodeId);

    void OnTimeLimitConcluded(Func<ChronicleEventNodeSpec, string?> getNodeId)
    {
        activeEvent.Clear();

        if (activeRef is null)
        {
            // Should not happen
            return;
        }

        var (node, controller) = activeRef.Value;
        activeRef = null;
        controller.TriggerNode(getNodeId(node));
    }

    public void RestoreGameState(ChronicleEventNodeSpec node, SpecChronicleEventController controller)
    {
        var data = node.GetData<TimeLimitData>();
        activeRef = (node, controller);

        var days = GetDays(data, controller);
        activeEvent.RegisterSavedTimeLimit(days, OnTimeLimitReached);

        if (activeEvent.HasSavedPayment)
        {
            activeEvent.RegisterSavedPayment(OnPaymentPaid);
        }
    }

    static float GetDays(TimeLimitData data, SpecChronicleEventController controller)
    {
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
