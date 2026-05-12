namespace BeaverChronicles.Events.Implementations;

public class FirstChildGrowth(
    ChronicleEventUIHelper uiHelper,
    ILoc t,
    ActiveChronicleEventService activeService,
    CharacterStatusHelper characterStatusHelper,
    PopulationHelper populationHelper
) : SavableChronicleEventBase(activeService)
{
    const float PartyDays = 1f;
    const float BuffDays = 3f;
    const float BuffAmount = .1f;
    const string BuffId = nameof(FirstChildGrowth) + "_Party";

    static readonly object[] ChoiceFormats = [PartyDays, "Berries", 1, "Water", 1];

    public override IReadOnlyCollection<EventTriggerSource> TriggerSources { get; } = [EventTriggerSource.BeaverGrownUp];

    string BeaverName
    {
        get => historyRecord!.CustomParameters["Name"];
        set => historyRecord!.CustomParameters["Name"] = value;
    }

    public override int GetTriggerWeight(IEventTriggerParameters parameters, ChronicleEventService chronicleEventService)
        => parameters.GetParameterWeight<BeaverGrownUpParameters>();

    int GetAmountToPay(int perBeaver)
    {
        var pop = populationHelper.GetPopulationData();
        return (pop.NumberOfAdults + (pop.NumberOfChildren + 1) / 2) * perBeaver;
    }

    void GatherPayment(object[] requirements)
    {
        activeService.SetActiveDescription(t.TEventChoice(Id, 0));
        activeService.RegisterPayment(OnPaymentFinished, GetPayment());
        activeService.RegisterTimeLimit((float)requirements[0], OnFailed);

        IEnumerable<GoodAmount> GetPayment()
        {
            for (int i = 1; i < requirements.Length; i += 2)
            {
                yield return new((string)requirements[i], (int)requirements[i + 1]);
            }
        }
    }

    void OnPaymentFinished()
    {
        if (historyRecord is null || historyRecord.Id != Id)
        {
            return;
        }

        var buffDesc = t.T("LV.BCEv.WorkingSpeedDesc", BuffAmount);
        var status = new LimitedTimeCharacterStatus(
            BuffId,
            CharacterType.AdultBeaver | CharacterType.ChildBeaver,
            [new(BonusType.WorkingSpeed, BuffAmount)],
            t.T("LV.BCEv.FirstChildGrowth.BuffName"),
            buffDesc);

        characterStatusHelper.AddOrUpdateLimitedTimeBonus(status, BuffDays);

        var content = t.TEventOutcome(Id, 0).Format(BeaverName, buffDesc, BuffDays);
        historyRecord.AddPage(side: true).AddContent(content);

        Conclude();

        uiHelper.ShowDismissOnlyChoiceDialog(this, b => b
            .SetTextContent(content)
            .SetSideImage()
        );
    }

    void OnFailed()
    {
        if (historyRecord is null || historyRecord.Id != Id)
        {
            return;
        }

        var content = t.TEventOutcome(Id, 1);
        historyRecord.AddPage(side: true).AddContent(content);

        Conclude();

        uiHelper.ShowDismissOnlyChoiceDialog(this, b => b
            .SetTextContent(content)
            .SetSideImage()
        );
    }

    protected override void OnTriggeredFromLoad(int[] choices, IEventTriggerParameters parameters, EventHistoryRecord record)
    {
        if (activeService.SavedPayment is not null)
        {
            activeService.RegisterSavedPayment(OnPaymentFinished);
        }

        if (activeService.SavedProgress is not null)
        {
            activeService.SetActiveDescription(t.TEventChoice(Id, 0));
            activeService.RegisterSavedTimeLimit((float)ChoiceFormats[0], OnFailed);
        }
    }

    protected override async void OnNewlyTriggered(IEventTriggerParameters parameters, EventHistoryRecord record)
    {
        var page = historyRecord!.AddPage(side: true);
        var name = parameters.GetParameter<BeaverGrownUpParameters>().Adult.FirstName;
        BeaverName = name;

        var content = t.TEventContent(Id).Format(name);
        page.AddContent(content);

        var format = ChoiceFormats.ToArray();
        format[2] = GetAmountToPay((int)format[2]);
        format[4] = GetAmountToPay((int)format[4]);

        var choices = SimpleChoiceData.Create(2, Id, t,
            (i, n) => i == 0 ? n.Format(format) : n
        );

        var index = await uiHelper.ShowChoiceDialogAsync(this, b => b
            .SetTextContent(content)
            .SetSideImage()
            .AddChoices(choices)
            .SetDefaultChoice(1)
        );

        choices[index].Record(page);

        switch (index)
        {
            case 0:
                GatherPayment(format);
                break;
            case 1:
                Conclude();
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }
}
