
namespace BeaverChronicles.Events;

public interface IChronicleEvent
{

    string Id { get; }
    IReadOnlyCollection<EventTriggerSource> TriggerSources { get; }
    bool CanRepeat => false;

    bool Active { get; }

    /// <summary>
    /// Return the weight of this event being triggered right now. It will be considered against other eligible events.
    /// </summary>
    /// <returns>
    /// -1: this event will never be triggered again, including CanRepeat events.<br />
    /// &lt;= 0: it will not be triggered this time.<br />
    /// <see cref="int.MaxValue"/>: this event will surely be triggered unless other events return the same weight.
    /// In that case, one of them will be randomly chosen.<br />
    /// Other values: the higher the weight, the more likely this event will be triggered. Use 100 for a "standard" value.
    /// </returns>
    int GetTriggerWeight(IEventTriggerParameters parameters);

    /// <summary>
    /// Handle the control flow over to this event instance.
    /// </summary>
    /// <remarks>
    /// Also called when this is the current event and the game is loaded, with parameters being EventTriggerSource.GameLoad.
    /// </remarks>
    void Trigger(IEventTriggerParameters parameters, ChronicleEventService chronicleEventService);

}
