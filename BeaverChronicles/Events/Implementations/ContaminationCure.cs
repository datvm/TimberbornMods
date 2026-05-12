namespace BeaverChronicles.Events.Implementations;

public class ContaminationCure(
    ChronicleEventUIHelper uiHelper,
    ILoc t,
    ActiveChronicleEventService activeService,
    GoodsHelper goodsHelper,
    ScienceService scienceService,
    IGoodService goods,
    CharacterStatusHelper characterStatusHelper
) : SavableChronicleEventBase(activeService)
{
    public const string EventId = nameof(ContaminationCure);
    public override string Id => EventId;

    const float TriggerChance = .75f;
    const string WaterId = "Water";
    const string SunflowerId = "SunflowerSeeds";
    const string AntidoteId = "Antidote";
    const int WaterCost = 20;
    const int SunflowerCost = 20;
    const int ScienceCost = 50;
    const int AntidoteReward = 100;
    const int FailureContaminationCount = 3;
    const float ShortPaymentDays = .5f;
    const float LongPaymentDays = 1f;
    const float SingleTreatmentSuccessChance = .4f;
    const float AssistedTreatmentSuccessChance = .1f;

    bool checkedForGoods = false;

    static readonly object[][] ChoiceFormats =
    [
        [ShortPaymentDays, WaterId, WaterCost, ],
        [ShortPaymentDays, SunflowerId, SunflowerCost],
        [LongPaymentDays, WaterId, WaterCost, SunflowerId, SunflowerCost, ActiveEventPayment.ScienceId, ScienceCost],
        []
    ];

    string CharacterName
    {
        get => GetParameter("CharacterName");
        set => SetParameter("CharacterName", value);
    }

    string CharacterEntityId
    {
        get => GetParameter("CharacterEntityId");
        set => SetParameter("CharacterEntityId", value);
    }

    public override IReadOnlyCollection<EventTriggerSource> TriggerSources => [EventTriggerSource.Contaminated];

    public override int GetTriggerWeight(IEventTriggerParameters parameters, ChronicleEventService chronicleEventService)
    {
        if (!checkedForGoods) // Only trigger for FT or any modded faction that has the required goods
        {
            var valid = goods.HasGood(SunflowerId) && goods.HasGood(AntidoteId) && goods.HasGood(WaterId);

            if (!valid)
            {
                return -1;
            }

            checkedForGoods = true;
        }

        var p = parameters.GetParameterOrDefault<CharacterParameters>();
        return p is not null && p.IsBeaver && BeaverChroniclesUtils.Chance(TriggerChance) ? 100 : 0;
    }

    void GatherPayment(int index)
    {
        var timeLimit = GetPaymentTimeLimit(index);
        var goods = GetPayment(index);

        activeService.SetActiveDescription(t.TEventChoice(Id, index));
        activeService.RegisterPayment(OnPaymentFinished, goods);
        activeService.RegisterTimeLimit(timeLimit, OnPaymentFailed);
    }

    void OnPaymentFinished()
    {
        if (historyRecord is null || historyRecord.Id != Id)
        {
            return;
        }

        if (!historyRecord.TryGetChoice(0, out var index))
        {
            throw new InvalidOperationException("The contamination cure event has no recorded treatment choice.");
        }

        var successChance = index == 2 ? AssistedTreatmentSuccessChance : SingleTreatmentSuccessChance;
        if (BeaverChroniclesUtils.Chance(successChance))
        {
            OnTreatmentSucceeded();
        }
        else
        {
            OnTreatmentFailed(index == 2);
        }
    }

    void OnPaymentFailed() => OnTreatmentFailed(false);

    void OnTreatmentSucceeded()
    {
        characterStatusHelper.CureContamination(Guid.Parse(CharacterEntityId));
        goodsHelper.GiveToDistrictCenter([new(AntidoteId, AntidoteReward)]);

        var imgPath = ChronicleEventUIHelper.GetSideImagePath(Id + "_O1");
        var page = historyRecord!.AddPage(sideImagePath: imgPath);
        var content = t.TEventOutcome(Id, 0).Format(CharacterName, AntidoteId, AntidoteReward);
        page.AddContent(content);

        uiHelper.ShowDismissOnlyChoiceDialog(this, b => b
            .SetTextContent(content)
            .SetSideImage(imgPath)
        );

        Conclude();
    }

    void OnTreatmentFailed(bool hasScience)
    {
        int outcomeIndex;
        if (hasScience)
        {
            scienceService.AddPoints(ScienceCost);
            outcomeIndex = 2;
        }
        else
        {
            characterStatusHelper.FindAndContaminateRandomBeavers(FailureContaminationCount);
            outcomeIndex = 1;
        }
        
        var page = historyRecord!.AddPage(top: true);
        var content = t.TEventOutcome(Id, outcomeIndex).Format(CharacterName, hasScience ? ScienceCost : FailureContaminationCount);
        page.AddContent(content);

        uiHelper.ShowDismissOnlyChoiceDialog(this, b => b
            .SetTextContent(content)
            .SetTopImage()
        );

        Conclude();
    }

    protected override void OnTriggeredFromLoad(int[] choices, IEventTriggerParameters parameters, EventHistoryRecord record)
    {
        if (activeService.SavedPayment is not null)
        {
            activeService.RegisterSavedPayment(OnPaymentFinished);
        }

        if (activeService.SavedProgress is not null)
        {
            activeService.RegisterSavedTimeLimit(GetPaymentTimeLimit(choices[0]), OnPaymentFailed);
        }
    }

    protected override async void OnNewlyTriggered(IEventTriggerParameters parameters, EventHistoryRecord record)
    {
        var character = parameters.GetParameter<CharacterParameters>().Character;
        CharacterName = character.FirstName;
        CharacterEntityId = character.GetEntityId().ToString();

        var page = record.AddPage(top: true);

        var content = t.TEventContent(Id).Format(CharacterName);
        page.AddContent(content);

        var choices = SimpleChoiceData.Create(4, Id, t,
            (i, n) => n.Format(ChoiceFormats[i])
        );

        var index = await uiHelper.ShowChoiceDialogAsync(this, b => b
            .SetTextContent(content)
            .SetTopImage()
            .AddChoices(choices)
            .SetDefaultChoice(3)
        );

        record.RecordChoice(0, index);
        var c = choices[index];
        c.Record(page);
        activeService.SetActiveDescription(c.Text);

        switch (index)
        {
            case 0 or 1 or 2:
                GatherPayment(index);
                break;
            case 3:
                Conclude();
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    static IEnumerable<GoodAmount> GetPayment(int choice) => choice switch
    {
        0 => [new(WaterId, WaterCost)],
        1 => [new(SunflowerId, SunflowerCost)],
        2 => [new(WaterId, WaterCost), new(SunflowerId, SunflowerCost), new(ActiveEventPayment.ScienceId, ScienceCost)],
        _ => throw new ArgumentException(),
    };

    static float GetPaymentTimeLimit(int choice) => (float)ChoiceFormats[choice][0];

}
