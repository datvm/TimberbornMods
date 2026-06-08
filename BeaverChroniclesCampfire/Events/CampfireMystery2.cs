namespace BeaverChroniclesCampfire.Events;

public class CampfireMystery2(
    ActiveChronicleEventService activeService,
    ILoc t,
    IDayNightCycle dayNightCycle,
    ChronicleEventUIHelper uiHelper
) : SavableChronicleEventBase(activeService)
{
    static readonly object[][] ChoiceFormats = [
        ["Log", 20],
        ["Berries", 20],
        ["Science", 25],
    ];

    public override string Id => CampfireUtils.Chapter2Id;
    public override float? DelayAfterConclusion => 0;

    public override IReadOnlyCollection<EventTriggerSource> TriggerSources => [EventTriggerSource.NewDay];

    public override int GetTriggerWeight(ChronicleEventContext context)
        => context.GetCampfireTriggerWeight(1);

    protected override void OnNewlyTriggered(IEventTriggerParameters parameters, EventHistoryRecord record)
        => activeService.WaitUntilNighttime(OnNightTime, dayNightCycle, t);

    protected override void OnTriggeredFromLoad(int[] choices, IEventTriggerParameters parameters, EventHistoryRecord record)
    {
        switch (choices.Length)
        {
            case 0:
                activeService.ClearSaved();
                activeService.WaitUntilNighttime(OnNightTime, dayNightCycle, t);
                break;
            case 1:
                activeService.RegisterSavedPayment(OnPaymentGathered);
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    async void OnNightTime()
    {
        var firstChapterRecord = context!.GetCampfireRecord(0);
        var ch1Choice = firstChapterRecord.GetChoice(0);
        var ch1Key = GetChapter1ChoiceKey(ch1Choice);

        var content = t.TEventContent(Id).Format(
            t.T($"LV.BCEv.{Id}.Content.{ch1Key}"),
            t.T($"LV.BCEv.{Id}.Content.Clue{ch1Key}"),
            t.T($"LV.BCEv.{Id}.Content.Tone{ch1Key}")
        );

        historyRecord!.RecordChoice(0, ch1Choice);
        var page = historyRecord.AddCampfirePage(top: true);
        page.AddContent(content);

        var choice = new SimpleChoiceData(
            t.T($"LV.BCEv.{Id}.C3.{ch1Key}"),
            t.T($"LV.BCEv.{Id}.C3N.{ch1Key}").Format(ChoiceFormats[ch1Choice][1])
        );

        await uiHelper.ShowChoiceDialogAsync(this, b => b
            .SetTextContent(content)
            .SetCampfireTopImage()
            .AddChoices([choice])
        );

        choice.Record(page);

        activeService.SetActiveDescription(choice.Text);
        activeService.RegisterPayment(OnPaymentGathered, ChoiceFormats[ch1Choice].ToGoods(0));
    }

    void OnPaymentGathered()
    {
        var ch1Choice = historyRecord!.GetChoice(0);
        var ch1Key = GetChapter1ChoiceKey(ch1Choice);
        var outcomeText = t.T($"LV.BCEv.{Id}.O3.{ch1Key}");

        var page = historyRecord.AddCampfirePage(side: true);
        page.AddContent(outcomeText);

        uiHelper.ShowDismissOnlyChoiceDialog(this, b => b
            .SetTextContent(outcomeText)
            .SetCampfireSideImage()
        );

        Conclude();
    }

    static string GetChapter1ChoiceKey(int choice) => $"Ch1C{choice + 1}";

    protected override void OnConcluded()
    {
        base.OnConcluded();
        context!.RequestNextEvent(CampfireUtils.Chapter3Id);
    }

}
