namespace BeaverChronicles.Events.Implementations;

public class DroughtPrep(
    ChronicleEventUIHelper uiHelper,
    ILoc t,
    ActiveChronicleEventService activeService,
    GoodsHelper goodsHelper
) : SavableChronicleEventBase(activeService), IChronicleEvent
{
    public override IReadOnlyCollection<EventTriggerSource> TriggerSources => [EventTriggerSource.WeatherWarning];

    const float TriggerChance = .5f;
    const float DelayDuration = 1f;
    const float FailChance = .4f;
    static readonly GoodAmount[] PossibleRewards = [new("Water", 20), new("Log", 10), new("Berries", 20)];

    public override float? DelayAfterConclusion => null;
    bool IChronicleEvent.CanRepeat => true;

    public override int GetTriggerWeight(IEventTriggerParameters parameters, ChronicleEventService chronicleEventService)
    {
        var p = parameters.GetParameterOrDefault<WeatherWarningParameters>();
        return p is null || p.HazardousWeatherId != CompatWeatherService.DroughtId || !BeaverChroniclesUtils.Chance(TriggerChance)
            ? 0
            : int.MaxValue;
    }

    void RegisterSearch(int index, string description)
    {
        activeService.SetActiveDescription(description);
        activeService.RegisterTimeLimit(DelayDuration, OnSearchFinished);
    }

    void OnSearchFinished()
    {
        if (historyRecord is null || historyRecord.Id != Id)
        {
            return;
        }

        if (!historyRecord.TryGetChoice(0, out var index))
        {
            throw new InvalidOperationException("The drought preparation event has no recorded search choice.");
        }

        var success = !BeaverChroniclesUtils.Chance(FailChance);
        var outcomeIndex = success ? index : index + 3;
        var content = t.TEventOutcome(Id, outcomeIndex);

        if (success)
        {
            var reward = PossibleRewards[index];
            goodsHelper.GiveToDistrictCenter([reward]);
            content = content.Format(reward.GoodId, reward.Amount);
        }

        historyRecord.AddPage().AddContent(content);

        Conclude();

        uiHelper.ShowDismissOnlyChoiceDialog(this, b => b
            .SetTextContent(content)
        );
    }

    protected override void OnTriggeredFromLoad(int[] choices, IEventTriggerParameters parameters, EventHistoryRecord record)
    {
        if (activeService.SavedProgress is not null)
        {
            activeService.RegisterSavedTimeLimit(DelayDuration, OnSearchFinished);
        }
    }

    protected override async void OnNewlyTriggered(IEventTriggerParameters parameters, EventHistoryRecord record)
    {
        var page = record.AddPage(top: true);

        var content = t.TEventContent(Id);
        page.AddContent(content);

        var choices = SimpleChoiceData.CreateNoNote(3, Id, t);

        var index = await uiHelper.ShowChoiceDialogAsync(this, b => b
            .SetTextContent(content)
            .AddChoices(choices)
            .SetTopImage()
        );

        record.RecordChoice(0, index);
        choices[index].Record(page);

        RegisterSearch(index, choices[index].Text);
    }

}
