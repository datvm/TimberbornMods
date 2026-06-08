namespace BeaverChronicles.Events.Implementations;

public class FoodWoodDispute2(
    ChronicleEventUIHelper uiHelper,
    ILoc t,
    WorkplaceHelper workplaceHelper,
    ActiveChronicleEventService activeService
) : SavableChronicleEventBase(activeService)
{
    public const string EventId = nameof(FoodWoodDispute2);
    public override string Id => EventId;


    /*
     * Chapter 2:
     * - Side with the same group as Chapter 1 (choosing 1st or 2nd that was chosen in Chapter 1):
     *   - Favored group gains Ch2LoyalBonus Working Speed.
     *   - Opposing group loses Ch2LoyalMalus Working Speed.
     *
     * - Switch sides (choosing 1st or 2nd choice that is not the same as Chapter 1):
     *   - Ch2SwitchSuccessfulChance to stabilize the dispute.
     *   - On success: new favored group gains Ch2SwitchBonus, old favored group loses Ch2SwitchMalus.
     *   - On failure: both groups lose Ch2SwitchCrisisMalus.
     *
     * - Stay neutral (choosing 3rd choice, does not matter what was chosen in Chapter 1):
     *   - Ch2NeutralSuccessfulChance to resolve the dispute peacefully.
     *   - On success: both groups gain Ch2NeutralBonus.
     *   - On failure: both groups lose Ch2NeutralMalus.
     *
     * All Chapter 2 effects last Ch2Duration days.
     */

    public const float Ch2Duration = 3f;
    public const float Ch2LoyalBonus = 1f;
    public const float Ch2LoyalMalus = -.95f;
    public const float Ch2SwitchSuccessfulChance = .5f;
    public const float Ch2SwitchBonus = .15f;
    public const float Ch2SwitchMalus = -.15f;
    public const float Ch2SwitchCrisisMalus = -.95f;
    public const float Ch2NeutralSuccessfulChance = .5f;
    public const float Ch2NeutralBonus = .15f;
    public const float Ch2NeutralMalus = -.95f;

    const string OutcomeKey = "Outcome";
    const string FarmhouseStatusId = nameof(FoodWoodDispute2) + "_Farmhouse";
    const string LumberjackStatusId = nameof(FoodWoodDispute2) + "_Lumberjack";
    const string FarmerKey = "LV.BCEv.FoodWoodDispute2.Farmer";
    const string LumberjackKey = "LV.BCEv.FoodWoodDispute2.Lumberjack";

    public override IReadOnlyCollection<EventTriggerSource> TriggerSources => [ EventTriggerSource.NewDay, ];

    public override int GetTriggerWeight(ChronicleEventContext context)
    {
        // Check for the first event
        var firstRecord = GetFirstRecord(context);
        if (firstRecord is null)
        {
            return context.History.NextEventIdRequested == EventId ? -1 : 0;
        }

        if (!firstRecord.TryGetChoice(0, out var choice))
        {
            throw new InvalidOperationException("The first food and wood dispute event has no recorded choice.");
        }

        if (FoodWoodDispute1.WasNeutralSuccessful(choice, firstRecord)) { return -1; }

        return int.MaxValue;
    }

    static EventHistoryRecord? GetFirstRecord(ChronicleEventContext context)
        => context.History.Get(FoodWoodDispute1.EventId).FirstOrDefault();

    void OnTimeFinished()
    {
        workplaceHelper.RemoveWorkplaceBonus(LumberjackStatusId);
        workplaceHelper.RemoveWorkplaceBonus(FarmhouseStatusId);
        Conclude();
    }

    (float Lumberjack, float Farmhouse) ApplyBonuses(DisputeSide choice, DisputeOutcome outcome, float time)
    {
        var bonuses = GetBonusValues(choice, outcome);
        var title = t.T(NameLoc);

        ApplyBonus(LumberjackStatusId, WorkplaceHelper.IsLumberjack, bonuses.Lumberjack);
        ApplyBonus(FarmhouseStatusId, WorkplaceHelper.IsFarmhouse, bonuses.Farmhouse);

        return bonuses;

        void ApplyBonus(string id, WorkplaceHelper.WorkplaceFilter filter, float amount)
        {
            workplaceHelper.AddOrUpdateWorkplaceBonus(new(
                id,
                filter,
                [new(BonusType.WorkingSpeed, amount)],
                title,
                t.T("LV.BCEv.WorkingSpeedDesc", amount)
            ), time);
        }
    }

    static (float Lumberjack, float Farmhouse) GetBonusValues(DisputeSide choice, DisputeOutcome outcome) => outcome switch
    {
        DisputeOutcome.Loyal => choice == DisputeSide.Farmers
            ? (Ch2LoyalMalus, Ch2LoyalBonus)
            : (Ch2LoyalBonus, Ch2LoyalMalus),
        DisputeOutcome.SwitchSuccess => choice == DisputeSide.Farmers
            ? (Ch2SwitchMalus, Ch2SwitchBonus)
            : (Ch2SwitchBonus, Ch2SwitchMalus),
        DisputeOutcome.SwitchCrisis => (Ch2SwitchCrisisMalus, Ch2SwitchCrisisMalus),
        DisputeOutcome.NeutralSuccess => (Ch2NeutralBonus, Ch2NeutralBonus),
        DisputeOutcome.NeutralFailure => (Ch2NeutralMalus, Ch2NeutralMalus),
        _ => throw new ArgumentOutOfRangeException(nameof(outcome)),
    };

    static DisputeOutcome GetOutcome(DisputeSide choice, DisputeSide previousSide)
    {
        if (choice is DisputeSide.Farmers or DisputeSide.Lumberjacks)
        {
            if (choice == previousSide)
            {
                return DisputeOutcome.Loyal;
            }

            return BeaverChroniclesUtils.Chance(Ch2SwitchSuccessfulChance)
                ? DisputeOutcome.SwitchSuccess
                : DisputeOutcome.SwitchCrisis;
        }

        if (choice == DisputeSide.Neutral)
        {
            return BeaverChroniclesUtils.Chance(Ch2NeutralSuccessfulChance)
                ? DisputeOutcome.NeutralSuccess
                : DisputeOutcome.NeutralFailure;
        }

        throw new ArgumentOutOfRangeException(nameof(choice));
    }

    object[] GetOutcomeFormats(DisputeSide choice, DisputeOutcome outcome, float lumberjack, float farmhouse) => outcome switch
    {
        DisputeOutcome.Loyal or DisputeOutcome.SwitchSuccess => choice == DisputeSide.Farmers
            ? [Ch2Duration, farmhouse, lumberjack, t.T(FoodWoodDispute2.FarmerKey), t.T(FoodWoodDispute2.LumberjackKey)]
            : [Ch2Duration, lumberjack, farmhouse, t.T(FoodWoodDispute2.LumberjackKey), t.T(FoodWoodDispute2.FarmerKey)],
        DisputeOutcome.SwitchCrisis => [Ch2Duration, lumberjack, farmhouse, t.T(LumberjackKey), t.T(FarmerKey)],
        DisputeOutcome.NeutralSuccess or DisputeOutcome.NeutralFailure => [Ch2Duration, lumberjack],
        _ => throw new ArgumentOutOfRangeException(nameof(outcome)),
    };

    static DisputeSide ParseSide(int choiceIndex) => choiceIndex switch
    {
        0 => DisputeSide.Farmers,
        1 => DisputeSide.Lumberjacks,
        2 => DisputeSide.Neutral,
        _ => throw new ArgumentOutOfRangeException(nameof(choiceIndex)),
    };

    static DisputeOutcome ParseOutcome(string value)
    {
        if (Enum.TryParse(value, out DisputeOutcome outcome))
        {
            return outcome;
        }

        throw new InvalidOperationException("Invalid save state");
    }

    protected override void OnTriggeredFromLoad(int[] choices, IEventTriggerParameters parameters, EventHistoryRecord record)
    {
        if (choices.Length == 0
            || !record.CustomParameters.TryGetValue(OutcomeKey, out var outcomeText))
        {
            throw new InvalidOperationException("Invalid save state");
        }

        var choice = ParseSide(choices[0]);
        var outcome = ParseOutcome(outcomeText);

        activeService.RegisterSavedTimeLimit(Ch2Duration, OnTimeFinished);
        var remainingTime = activeService.RemainingDays;

        ApplyBonuses(choice, outcome, remainingTime);
    }

    protected override async void OnNewlyTriggered(IEventTriggerParameters parameters, EventHistoryRecord record)
    {
        var firstRecord = GetFirstRecord(context!)
            ?? throw new InvalidOperationException("The first food and wood dispute event has no record.");

        if (!firstRecord.TryGetChoice(0, out var firstChoice))
        {
            throw new InvalidOperationException("The first food and wood dispute event has no recorded choice.");
        }

        var previousSide = ParseSide(firstChoice);

        var page = record.AddPage(topImagePath: FoodWoodDispute1.TopImagePath);

        var content = t.TEventContent(Id);
        page.AddContent(content);

        var choices = SimpleChoiceData.Create(3, Id, t, _ => t.TUnknownConsequences());
        var index = await uiHelper.ShowChoiceDialogAsync(this, b => b
            .SetTextContent(content)
            .SetTopImage(FoodWoodDispute1.TopImagePath)
            .AddChoices(choices)
            .SetDefaultChoice(2)
        );

        var choiceText = choices[index].Text;
        record.RecordChoice(0, index);
        page.AddContent(choiceText.CenterMixed());

        var choice = ParseSide(index);
        var outcome = GetOutcome(choice, previousSide);
        record.CustomParameters[OutcomeKey] = outcome.ToString();

        var (lumberjack, farmhouse) = ApplyBonuses(choice, outcome, Ch2Duration);
        activeService.RegisterTimeLimit(Ch2Duration, OnTimeFinished);
        activeService.SetActiveDescription(choiceText);

        var outcomeText = t.TEventOutcome(Id, (int)outcome)
            .Format(GetOutcomeFormats(choice, outcome, lumberjack, farmhouse));
        page = record.AddPage(topImagePath: FoodWoodDispute1.TopImagePath);
        page.AddContent(outcomeText);

        uiHelper.ShowDismissOnlyChoiceDialog(this, b => b
            .SetTextContent(outcomeText)
            .SetTopImage(FoodWoodDispute1.TopImagePath)
        );
    }


    enum DisputeSide
    {
        Farmers,
        Lumberjacks,
        Neutral,
    }

    enum DisputeOutcome
    {
        Loyal,
        SwitchSuccess,
        SwitchCrisis,
        NeutralSuccess,
        NeutralFailure,
    }
}
