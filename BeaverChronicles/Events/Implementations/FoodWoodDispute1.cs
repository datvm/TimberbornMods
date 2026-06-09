namespace BeaverChronicles.Events.Implementations;

public class FoodWoodDispute1(
    ChronicleEventUIHelper uiHelper,
    ILoc t,
    GameCycleService gameCycleService,
    WorkplaceHelper workplaceHelper,
    ActiveChronicleEventService activeService
) : SavableChronicleEventBase(activeService)
{
    /*
     * Chapter 1:
     * - Side with either side:
     *   - + side gain Ch1Bonus Working Speed.
     *   - - side Flags lose Ch1Malus Working Speed.
     *   - Trigger Chapter 2 after Ch1Duration.
     *
     * - Stay Neutral:
     *   - 30%:
     *     - Both sides calm down.
     *     - Both gain Ch1Bonus Working Speed.
     *     - Event chain ends.
     *
     *   - 70%:
     *     - Both sides remain unhappy.
     *     - Both lose Ch1Malus Working Speed.
     *     - Trigger Chapter 2 after Ch1Duration.
     */

    public const string EventId = nameof(FoodWoodDispute1);
    public override string Id => EventId;

    public const string NeutralOutcomeKey = "NeutralOutcome";
    public const string TopImagePath = ChronicleEventUIHelper.RecommendedImagePath + "FoodWoodDispute_Top";

    const int MinCycle = 2;
    const int MinJobs = 5;

    const float Ch1Duration = 2f;
    const float Ch1Bonus = .2f; 
    const float Ch1Malus = -.2f;
    const float NeutralSuccessfulChance = .3f;

    const string FarmhouseStatusId = nameof(FoodWoodDispute1) + "_Farmhouse";
    const string LumberjackStatusId = nameof(FoodWoodDispute1) + "_Lumberjack";

    public override float? DelayAfterConclusion => 0;

    public override IReadOnlyCollection<EventTriggerSource> TriggerSources => [EventTriggerSource.NewDay];

    public override int GetTriggerWeight(ChronicleTriggerContext context)
    {
        if (gameCycleService.Cycle < MinCycle) { return 0; }

        if (!workplaceHelper.HasMinimumWorkers(MinJobs, WorkplaceHelper.IsLumberjack)
            || !workplaceHelper.HasMinimumWorkers(MinJobs, WorkplaceHelper.IsFarmhouse)) { return 0; }

        return 100;
    }

    (float Lumberjack, float Farmhouse) ApplyBonuses(int index, bool neutralOutcome, float time)
    {
        var bonusses = GetBonusValues(index, neutralOutcome);

        var title = t.T(NameLoc);
        ApplyBonus(LumberjackStatusId, WorkplaceHelper.IsLumberjack, bonusses.Lumberjack);
        ApplyBonus(FarmhouseStatusId, WorkplaceHelper.IsFarmhouse, bonusses.Farmhouse);

        return bonusses;

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

    void OnTimeFinished()
    {
        workplaceHelper.RemoveWorkplaceBonus(LumberjackStatusId);
        workplaceHelper.RemoveWorkplaceBonus(FarmhouseStatusId);

        // Chapter 2 should trigger next day
        context!.RequestNextEvent(FoodWoodDispute2.EventId);
        Conclude();
    }

    static (float Lumberjack, float Farmhouse) GetBonusValues(int choice, bool neutralOutcome) => choice switch
    {
        0 => (Ch1Malus, Ch1Bonus),
        1 => (Ch1Bonus, Ch1Malus),
        2 when neutralOutcome => (Ch1Bonus, Ch1Bonus),
        2 => (Ch1Malus, Ch1Malus),
        _ => throw new ArgumentOutOfRangeException(),
    };

    public static bool WasNeutralSuccessful(int choice, EventHistoryRecord record) 
        => choice == 2 && record.CustomParameters.TryGetValue(NeutralOutcomeKey, out var outcome) && outcome == true.ToString();

    protected override void OnTriggeredFromLoad(int[] choices, IEventTriggerParameters parameters, EventHistoryRecord record)
    {
        if (choices.Length == 0)
        {
            throw new InvalidOperationException("Invalid save state");
        }

        activeService.RegisterSavedTimeLimit(Ch1Duration, OnTimeFinished);
        var remainingTime = activeService.RemainingDays;

        var index = choices[0];
        var neutralOutcome = WasNeutralSuccessful(index, record);
        ApplyBonuses(index, neutralOutcome, remainingTime);
    }

    protected override async void OnNewlyTriggered(IEventTriggerParameters parameters, EventHistoryRecord record)
    {
        var page = record.AddPage(topImagePath: TopImagePath);

        var content = t.TEventContent(Id);
        page.AddContent(content);

        var unknownCons = t.TUnknownConsequences();
        var choices = SimpleChoiceData.Create(3, Id, t, _ => unknownCons);

        var index = await uiHelper.ShowChoiceDialogAsync(this, b => b
            .SetTextContent(content)
            .SetTopImage(TopImagePath)
            .AddChoices(choices)
            .SetDefaultChoice(2)
        );
        var choiceText = choices[index].Text;

        record.RecordChoice(0, index);
        page.AddContent(choiceText.CenterMixed());

        var outcomeIndex = index;
        var neutralOutcome = false;
        if (index == 2)
        {
            neutralOutcome = BeaverChroniclesUtils.Chance(NeutralSuccessfulChance);
            record.CustomParameters[NeutralOutcomeKey] = neutralOutcome.ToString();

            if (!neutralOutcome)
            {
                outcomeIndex++;
            }
        }

        var (lumberjack, farmhouse) = ApplyBonuses(index, neutralOutcome, Ch1Duration);
        activeService.RegisterTimeLimit(Ch1Duration, OnTimeFinished);
        activeService.SetActiveDescription(choiceText);

        var outcomeText = t.TEventOutcome(Id, outcomeIndex).Format(Ch1Duration, lumberjack, farmhouse);
        page = record.AddPage(topImagePath: TopImagePath);
        page.AddContent(outcomeText);

        uiHelper.ShowDismissOnlyChoiceDialog(this, b => b
            .SetTextContent(outcomeText)
            .SetTopImage(TopImagePath)
        );
    }
}
