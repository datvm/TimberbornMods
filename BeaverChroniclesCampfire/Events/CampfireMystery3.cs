namespace BeaverChroniclesCampfire.Events;

public class CampfireMystery3(
    ActiveChronicleEventService activeService,
    ILoc t,
    IDayNightCycle dayNightCycle,
    ChronicleEventUIHelper uiHelper,
    CharacterStatusHelper characterStatusHelper
) : SavableChronicleEventBase(activeService)
{
    static readonly string CapedChewerImgPath = ChronicleEventUIHelper.RecommendedImagePath + "CampfireMystery_CapedChewer";

    public override string Id => CampfireUtils.Chapter3Id;

    public override float? DelayAfterConclusion => 2;
    public override IReadOnlyCollection<EventTriggerSource> TriggerSources => [EventTriggerSource.NewDay];

    static readonly GoodAmount[][] FirstChoices = [
        [ new("Log", 1), new("PineResin", 1), ],
        [ new("Berries", 1), new("Science", 10), ],
        [new("Berries", 1), new("Log", 1), new("PineResin", 1), ],
    ];
    static readonly GoodAmount[][] OtherChoicesPayment = [
        [ new("Berries", 5), new("Water", 5), ],
        [ new("Plank", 20), ]
    ];
    

    public override int GetTriggerWeight(IEventTriggerParameters parameters, ChronicleEventService chronicleEventService)
        => chronicleEventService.GetCampfireTriggerWeight(2);

    protected override void OnNewlyTriggered(IEventTriggerParameters parameters, EventHistoryRecord record)
    {
        activeService.WaitUntilNighttime(OnNightTime, dayNightCycle, t);
    }

    protected override void OnTriggeredFromLoad(int[] choices, IEventTriggerParameters parameters, EventHistoryRecord record)
    {
        switch (choices.Length)
        {
            case 0:
                activeService.ClearSaved();
                activeService.WaitUntilNighttime(OnNightTime, dayNightCycle, t);
                break;
            case 2:
                activeService.RegisterSavedPayment(OnPaymentPaid);
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    async void OnNightTime()
    {
        var firstChoice = chronicleEventService!.GetCampfireRecord(0).GetChoice(0);
        historyRecord!.RecordChoice(0, firstChoice);

        var ch1Key = GetChapter1ChoiceKey(firstChoice);
        var content = t.TEventContent(Id).Format(t.T($"LV.BCEv.{Id}.Content.{ch1Key}"));

        var page = historyRecord.AddPage(topImagePath: CapedChewerImgPath);
        page.AddContent(content);

        var choices = CreateChoices(firstChoice, ch1Key);
        var index = await uiHelper.ShowChoiceDialogAsync(this, b => b
            .SetTextContent(content)
            .SetTopImage(CapedChewerImgPath)
            .AddChoices(choices)
        );

        historyRecord.RecordChoice(1, index);
        var choice = choices[index];
        choice.Record(page);

        activeService.SetActiveDescription(choice.Text);
        activeService.RegisterPayment(OnPaymentPaid, GetPayment(firstChoice, index));
    }

    void OnPaymentPaid()
    {
        var firstChoice = historyRecord!.GetChoice(0);
        var choice = historyRecord.GetChoice(1);

        var injuryCount = choice switch
        {
            0 => 1,
            1 or 2 => 3,
            _ => throw new ArgumentOutOfRangeException()
        };
        var outcomeText = GetOutcomeText(firstChoice, choice, injuryCount);

        var capedChewer = choice == 0;
        var page = capedChewer
            ? historyRecord.AddPage(topImagePath: CapedChewerImgPath)
            : historyRecord.AddCampfirePage(side: true);
        page.AddContent(outcomeText);

        characterStatusHelper.FindAndInjureRandomBeavers(injuryCount);

        uiHelper.ShowDismissOnlyChoiceDialog(this, b =>
        {
            b.SetTextContent(outcomeText);

            if (capedChewer)
            {
                b.SetTopImage(CapedChewerImgPath);
            }
            else
            {
                b.SetCampfireSideImage();
            }
        });

        Conclude();
    }

    SimpleChoiceData[] CreateChoices(int firstChoice, string ch1Key) => [
        new(
            t.TEventChoice(Id, 0),
            t.T($"LV.BCEv.{Id}.C1N.{ch1Key}").Format(FirstChoices[firstChoice].ToMixedText())
        ),
        new(
            t.TEventChoice(Id, 1),
            t.TEventChoiceNote(Id, 1).Format(OtherChoicesPayment[0].ToMixedText())
        ),
        new(
            t.TEventChoice(Id, 2),
            t.TEventChoiceNote(Id, 2).Format(OtherChoicesPayment[1].ToMixedText())
        ),
    ];

    string GetOutcomeText(int firstChoice, int choice, int injury)
    {
        var outcomeText = t.TEventOutcome(Id, choice);
        return choice == 0
            ? outcomeText.Format(t.T($"LV.BCEv.{Id}.O1.{GetChapter1ChoiceKey(firstChoice)}"), injury)
            : outcomeText.Format(injury);
    }

    protected override void OnConcluded()
    {
        base.OnConcluded();

        chronicleEventService!.RequestNextEvent(CampfireUtils.Chapter4Id);
    }

    static GoodAmount[] GetPayment(int firstChoice, int choice) => choice switch
    {
        0 => FirstChoices[firstChoice],
        1 or 2 => OtherChoicesPayment[choice - 1],
        _ => throw new ArgumentOutOfRangeException(nameof(choice)),
    };

    static string GetChapter1ChoiceKey(int choice) => $"Ch1C{choice + 1}";

}
