namespace BeaverChronicles.Services.TimeLimitEvents;

[MultiBind(typeof(ITimeLimitEvent))]
public class CharacterEvents(EntityRegistry entityRegistry, EventBus eb) : ITimeLimitEvent
{

    public IEnumerable<string> ForEvents => [
        TimeLimitEventsConstants.CharacterDied,
        TimeLimitEventsConstants.CharacterDecontaminated,
    ];

    public void SubscribeEvent(string name, SpecChronicleEventController controller)
    {
        var c = FindCharacter(controller);

        switch (name)
        {
            case TimeLimitEventsConstants.CharacterDied:
                c.Died += OnCharacterDied;
                break;
            case TimeLimitEventsConstants.CharacterDecontaminated:
                c.GetNeedManager().NeedChangedActiveState += CheckDecontamination;
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    
    public void UnsubscribeEvent(string name, SpecChronicleEventController controller)
    {
        var c = FindCharacter(controller);

        switch (name)
        {
            case TimeLimitEventsConstants.CharacterDied:
                c.Died -= OnCharacterDied;
                break;
            case TimeLimitEventsConstants.CharacterDecontaminated:
                c.GetNeedManager().NeedChangedActiveState -= CheckDecontamination;
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    Character FindCharacter(SpecChronicleEventController controller)
    {
        var rawId = controller.ActiveContext.Record.CustomParameters["TriggerCharacterId"]; // Fill in the key
        if (!Guid.TryParse(rawId, out var id))
        {
            throw new InvalidOperationException($"No TriggerCharacterId found in custom parameters.");
        }

        var character = entityRegistry.GetEntity(id)?.GetComponentOrNull<Character>();
        return character ?? throw new InvalidOperationException($"No character found with ID {id}.");
    }

    void OnCharacterDied(object sender, EventArgs e) => eb.Post(new OnTimeLimitEvent(TimeLimitEventsConstants.CharacterDied));

    void CheckDecontamination(object sender, NeedChangedActiveStateEventArgs e)
    {
        if (e.NeedSpec.Id == ChronicleGameEventHandler.ContaminationId && !e.IsActive)
        {
            eb.Post(new OnTimeLimitEvent(TimeLimitEventsConstants.CharacterDecontaminated));
        }
    }


}
