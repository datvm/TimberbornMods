namespace BeaverChroniclesCampfire.Events;

public class CampfireMystery1(
    ActiveChronicleEventService activeService,
    IDayNightCycle dayNightCycle,
    ILoc t,
    ChronicleEventUIHelper uiHelper
) : SavableChronicleEventBase(activeService)
{

    static readonly object[][] ChoiceFormats = [
        ["Log", 5],
        ["Berries", 5],
        ["Science", 2],
    ];

    public override string Id => CampfireUtils.Chapter1Id;

    public override float? DelayAfterConclusion => 0;

    public override IReadOnlyCollection<EventTriggerSource> TriggerSources => [EventTriggerSource.BuildingFinished];

    public override int GetTriggerWeight(IEventTriggerParameters parameters, ChronicleEventService chronicleEventService)
    {
        var p = parameters.GetParameterOrDefault<BuildingParameters>();
        if (p is null || !p.TemplateName.StartsWith("Campfire.")) { return 0; }

        return 100;
    }

    protected override void OnNewlyTriggered(IEventTriggerParameters parameters, EventHistoryRecord record)
    {
        var page = record.AddCampfirePage(top: true);
        
        if (dayNightCycle.IsNightTime())
        {
            OnNightTime();
            return;
        }
        
        var content = t.T("LV.BCEv.CampfireMystery.WaitPrompt", CampfireUtils.NightHour);
        page.AddContent(content);

        uiHelper.ShowDismissOnlyChoiceDialog(this, b => b
            .SetTextContent(content)
            .SetCampfireSideImage()
        );

        WaitUntilNighttime();
    }

    void WaitUntilNighttime() => activeService.WaitUntilNighttime(OnNightTime, dayNightCycle, t);

    protected override void OnTriggeredFromLoad(int[] choices, IEventTriggerParameters parameters, EventHistoryRecord record)
    {
        switch (choices.Length)
        {
            case 0: // Waiting for night time
                activeService.ClearSaved();
                WaitUntilNighttime();
                break;
            case 1: // Gathering payment
                activeService.RegisterSavedPayment(OnPaymentGathered);
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    async void OnNightTime()
    {
        var page = historyRecord!.CurrentPage;
        var content = t.TEventContent(Id);
        page.AddContent(content);

        var choices = SimpleChoiceData.Create(3, Id, t, (i, n) => n.Format(ChoiceFormats[i]));
        var index = await uiHelper.ShowChoiceDialogAsync(this, b => b
            .SetTextContent(content)
            .SetTopImage()
            .AddChoices(choices)
        );

        historyRecord.RecordChoice(0, index);
        var c = choices[index];
        c.Record(page);

        activeService.SetActiveDescription(c.Text);
        activeService.RegisterPayment(OnPaymentGathered, ChoiceFormats[index].ToGoods(0));
    }

    void OnPaymentGathered()
    {
        var choice = historyRecord!.GetChoice(0);
        var outcomeText = t.TEventOutcome(Id, choice);

        var page = historyRecord.AddCampfirePage(side: true);
        page.AddContent(outcomeText);

        uiHelper.ShowDismissOnlyChoiceDialog(this, b => b
            .SetTextContent(outcomeText)
            .SetCampfireSideImage()
        );

        Conclude();
    }

    protected override void OnConcluded()
    {
        base.OnConcluded();

        chronicleEventService!.RequestNextEvent(CampfireUtils.Chapter2Id);
    }

}
