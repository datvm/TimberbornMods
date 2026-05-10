namespace BeaverChronicles.Events.Implementations;

public class FirstDeath(ChronicleEventUIHelper uiHelper) : ChronicleEventBase
{
    public override IReadOnlyCollection<EventTriggerSource> TriggerSources { get; } = [EventTriggerSource.CharacterDeath,];

    public override int GetTriggerWeight(IEventTriggerParameters parameters)
    {
        var p = parameters.GetParameterOrDefault<CharacterParameters>();
        return (p is null || !p.IsBeaver) ? 0 : int.MaxValue;
    }

    public override async void OnTriggered(IEventTriggerParameters parameters)
    {
        var choice = await uiHelper.ShowEventDialogAsync(this, b => b
            .SetTopImage()
            .AddChoices(3)
        );


    }



    void OnFailed()
    {

    }

}
