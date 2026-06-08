namespace BeaverChronicles.Events.Implementations;

public class FirstDeath(
    ChronicleEventUIHelper uiHelper,
    ILoc t,
    ActiveChronicleEventService activeService,
    CharacterStatusHelper characterStatusHelper
) : SavableChronicleEventBase(activeService)
{
    const string BonusId = "Remembrance";
    const float NoConsequenceChance = 0.3f;
    const float Penalty = -.15f;
    const float PenaltyDays = 3f;
    static readonly object[][] ChoiceFormats = [[2f, "Log", 3], [5f, "Science", 10, "Plank", 20], []];

    string BeaverName
    {
        get => historyRecord!.CustomParameters["Name"];
        set => historyRecord!.CustomParameters["Name"] = value;
    }

    public override IReadOnlyCollection<EventTriggerSource> TriggerSources { get; } = [EventTriggerSource.CharacterDeath,];

    public override int GetTriggerWeight(ChronicleEventContext context)
    {
        var p = context.Parameters.GetParameterOrDefault<CharacterParameters>();
        return (p is null || !p.IsBeaver) ? 0 : int.MaxValue;
    }

    void GatherPayment(object[] requirements)
    {
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

    void DetermineIgnoredOutcome()
    {
        if (!BeaverChroniclesUtils.Chance(NoConsequenceChance))
        {
            OnFailed();
            return;
        }

        var text = t.TEventOutcome(Id, 3).Format(BeaverName);
        historyRecord!.AddPage(top: true).AddContent(text);

        uiHelper.ShowDismissOnlyChoiceDialog(this, b => b
            .SetTextContent(text)
            .SetTopImage()
        );
        Conclude();
    }

    void OnPaymentFinished()
    {
        if (!historyRecord!.TryGetChoice(0, out var index)) { throw new InvalidOperationException(); }

        var imgPath = ChronicleEventUIHelper.GetTopImagePath(Id + "_O" + (index + 1));
        var page = historyRecord.AddPage(topImagePath: imgPath);

        var content = t.TEventOutcome(Id, index).Format(BeaverName);
        page.AddContent(content);

        characterStatusHelper.BoostAllBeaversNeed([BonusId]);
        if (index == 1)
        {
            characterStatusHelper.PermanentNeedBoost.Add(BonusId);
        }

        Conclude();

        uiHelper.ShowDismissOnlyChoiceDialog(this, b => b
            .SetTextContent(content)
            .SetTopImage(imgPath)
        );
    }

    void OnFailed()
    {
        // Just to be sure, confirm it's still on
        if (historyRecord is null || historyRecord.Id != Id)
        {
            return;
        }

        var page = historyRecord.AddPage(top: true);

        var content = t.TEventOutcome(Id, 2).Format(BeaverName, PenaltyDays);
        page.AddContent(content);

        characterStatusHelper.AddOrUpdateLimitedTimeBonus(new(
            nameof(FirstDeath) + "_FailedPenalty", CharacterType.AdultBeaver, 
            [new(BonusType.WorkingSpeed, Penalty)],
            t.T("LV.BCEv.FirstDeath.MalusName"), t.T("LV.BCEv.WorkingSpeedDesc", Penalty)
        ), PenaltyDays);

        Conclude();

        uiHelper.ShowDismissOnlyChoiceDialog(this, b => b
            .SetTextContent(content)
            .SetTopImage()
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
            var days = (float)ChoiceFormats[choices[0]][0];
            activeService.RegisterSavedTimeLimit(days, OnFailed);
        }
    }

    protected override async void OnNewlyTriggered(IEventTriggerParameters parameters, EventHistoryRecord record)
    {
        var charName = parameters.GetParameter<CharacterParameters>().Character.FirstName;
        BeaverName = charName;

        var page = historyRecord!.AddPage(top: true);

        var content = t.T(ChronicleEventUIHelper.GetDefaultContentLoc(Id)).Format(charName);
        page.AddContent(content);

        var choices = SimpleChoiceData.Create(3, Id, t,
            (i, n) => n.Format(ChoiceFormats[i])
        );

        var index = await uiHelper.ShowChoiceDialogAsync(this, b => b
            .SetTextContent(content)
            .SetTopImage()
            .AddChoices(choices)
            .SetDefaultChoice(2)
        );

        record.RecordChoice(0, index);
        choices[index].Record(page);

        switch (index)
        {
            case 0 or 1:
                activeService.SetActiveDescription(choices[index].Text);
                GatherPayment(ChoiceFormats[index]);
                break;
            case 2:
                DetermineIgnoredOutcome();
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }
}
