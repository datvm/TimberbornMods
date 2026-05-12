namespace BeaverChronicles.Events;

public abstract class SavableChronicleEventBase(ActiveChronicleEventService activeService) : ChronicleEventBase
{

    protected readonly ActiveChronicleEventService activeService = activeService;

    protected override void OnTriggered(IEventTriggerParameters parameters, EventHistoryRecord record)
    {
        if (parameters.Source == EventTriggerSource.GameLoad)
        {
            OnTriggeredFromLoad(record.GetRecordedChoices(), parameters, record);

            if (activeService.SavedPayment is not null || activeService.SavedProgress is not null)
            {
                throw new InvalidOperationException($"The loaded data in {nameof(ActiveChronicleEventService)} is not processed yet. " +
                    $"Be sure to call their processing methods or {nameof(ActiveChronicleEventService.ClearSaved)}.");
            }
        }
        else
        {
            OnNewlyTriggered(parameters, record);
        }
    }

    protected abstract void OnTriggeredFromLoad(int[] choices, IEventTriggerParameters parameters, EventHistoryRecord record);
    protected abstract void OnNewlyTriggered(IEventTriggerParameters parameters, EventHistoryRecord record);

}
