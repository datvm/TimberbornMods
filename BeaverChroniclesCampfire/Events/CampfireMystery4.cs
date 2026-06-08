namespace BeaverChroniclesCampfire.Events;

public class CampfireMystery4(
    ActiveChronicleEventService activeService,
    ILoc t,
    ChronicleEventUIHelper uiHelper,
    CharacterStatusHelper characterStatusHelper,
    FloodingHelper floodingHelper,
    WellbeingService wellbeingService,
    WellbeingLimitService wellbeingLimitService,
    BeaverPopulation beaverPopulation,
    CharacterSpawnHelper characterSpawnHelper,
    GoodsHelper goodsHelper
) : SavableChronicleEventBase(activeService)
{
    int TargetWellbeing
    {
        get => int.Parse(GetParameter("TargetWellbeing"));
        set => SetParameter("TargetWellbeing", value.ToString());
    }

    const int WellbeingTargetDelta = 2;
    const float TrialDays = 5f;
    const float StatusDays = 5f;
    const float WelcomeBuffAmount = .1f;
    const float FailureDebuffAmount = -.15f;
    const string SuccessBuffId = nameof(CampfireMystery4) + "_Welcome";
    const string FailureDebuffId = nameof(CampfireMystery4) + "_FailedPenalty";

    static readonly GoodAmount[] SendAwayResources = [new("Berries", 20), new("Water", 20)];

    static readonly GoodAmount[] TrialRewardGoods = [new("TreatedPlank", 20), new("MetalBlock", 20)];

    public override string Id => CampfireUtils.Chapter4Id;

    public override IReadOnlyCollection<EventTriggerSource> TriggerSources => [EventTriggerSource.NewDay];

    public override int GetTriggerWeight(ChronicleEventContext context)
        => context.GetCampfireTriggerWeight(3);

    protected override async void OnNewlyTriggered(IEventTriggerParameters parameters, EventHistoryRecord record)
    {
        var ch3Choice = context!.GetCampfireRecord(2).GetChoice(1);
        historyRecord!.RecordChoice(0, ch3Choice);

        var ch3Key = GetChapter3ChoiceKey(ch3Choice);
        var content = t.TEventContent(Id).Format(
            t.T($"LV.BCEv.{Id}.Content.{ch3Key}"),
            t.T($"LV.BCEv.{Id}.Content.Trial{ch3Key}")
        );

        var page = historyRecord.AddCampfirePage(top: true);
        page.AddContent(content);

        if (ch3Choice == 0)
        {
            var curr = wellbeingService.AverageGlobalWellbeing;
            TargetWellbeing = Math.Min(curr + WellbeingTargetDelta, wellbeingLimitService.MaxBeaverWellbeing);
        }

        var choices = CreateChoices(ch3Key);
        var index = await uiHelper.ShowChoiceDialogAsync(this, b => b
            .SetTextContent(content)
            .SetCampfireTopImage()
            .AddChoices(choices)
        );

        historyRecord.RecordChoice(1, index);
        var c = choices[index];
        c.Record(page);

        activeService.SetActiveDescription(c.Text + Environment.NewLine + c.Note!.UncenterMixed());

        switch (index)
        {
            case 0:
                activeService.RegisterTimeLimit(TrialDays, OnSafetyTrialDue);
                SetupTrial(ch3Choice);
                break;
            case 1:
                activeService.RegisterTimeLimit(TrialDays, OnDriveOut);
                activeService.RegisterPayment(OnSendAwayPaymentPaid, SendAwayResources);
                break;
            case 2:
                OnDriveOut();
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(index));
        }
    }

    protected override void OnTriggeredFromLoad(int[] choices, IEventTriggerParameters parameters, EventHistoryRecord record)
    {
        if (choices.Length < 2)
        {
            throw new InvalidOperationException("The fourth campfire mystery event has no recorded choice.");
        }

        switch (choices[1])
        {
            case 0:
                activeService.RegisterSavedTimeLimit(TrialDays, OnSafetyTrialDue);
                SetupTrial(choices[0]);
                break;
            case 1:
                activeService.RegisterSavedPayment(OnSendAwayPaymentPaid);
                activeService.RegisterSavedTimeLimit(TrialDays, OnDriveOut);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(choices));
        }
    }

    void SetupTrial(int ch3Choice)
    {
        if (ch3Choice != 1) { return; }

        floodingHelper.OnBuildingFlooded += OnBuildingFlooded;
        floodingHelper.StartListen();
    }

    void OnBuildingFlooded(object sender, EventArgs e)
    {
        StopFloodingListening();

        if (!Active) { return; }

        activeService.ClearTimeLimit();
        OnSafetyTrialFailed();
    }

    void StopFloodingListening()
    {
        floodingHelper.StopListening();
        floodingHelper.OnBuildingFlooded -= OnBuildingFlooded;
    }

    void OnSafetyTrialDue()
    {
        StopFloodingListening();

        var ch3Choice = historyRecord!.GetChoice(0);
        if (IsSafetyTrialMet(ch3Choice))
        {
            OnSafetyTrialSucceeded();
        }
        else
        {
            OnSafetyTrialFailed();
        }
    }

    void OnSafetyTrialSucceeded()
    {
        var statusDescription = ApplySuccessBuff();
        var outcomeText = t.TEventOutcome(Id, 0).Format(
            statusDescription,
            StatusDays,
            TrialRewardGoods.ToMixedText()
        );

        var page = historyRecord!.AddCampfirePage(side: true);
        page.AddContent(outcomeText);

        uiHelper.ShowDismissOnlyChoiceDialog(this, b => b
            .SetTextContent(outcomeText)
            .SetCampfireSideImage()
        );

        if (characterSpawnHelper.FindAnySpawnSpot(out var location))
        {
            characterSpawnHelper.SpawnAdult(location);
        }
        goodsHelper.GiveToDistrictCenter(TrialRewardGoods);

        Conclude();
    }

    void OnSafetyTrialFailed()
    {
        var ch3Choice = historyRecord!.GetChoice(0);
        var ch3Key = GetChapter3ChoiceKey(ch3Choice);
        var statusDescription = ApplyFailureDebuff();
        var outcomeText = t.TEventOutcome(Id, 1).Format(
            t.T($"LV.BCEv.{Id}.O2.Fail{ch3Key}"),
            statusDescription,
            StatusDays
        );

        var page = historyRecord.AddCampfirePage(side: true);
        page.AddContent(outcomeText);

        uiHelper.ShowDismissOnlyChoiceDialog(this, b => b
            .SetTextContent(outcomeText)
            .SetCampfireSideImage()
        );

        Conclude();
    }

    void OnSendAwayPaymentPaid()
    {
        var outcomeText = t.TEventOutcome(Id, 2);

        var page = historyRecord!.AddCampfirePage(side: true);
        page.AddContent(outcomeText);

        uiHelper.ShowDismissOnlyChoiceDialog(this, b => b
            .SetTextContent(outcomeText)
            .SetCampfireSideImage()
        );

        Conclude();
    }

    void OnDriveOut()
    {
        var statusDescription = ApplyFailureDebuff();
        var outcomeText = t.TEventOutcome(Id, 3).Format(statusDescription, StatusDays);

        var page = historyRecord!.AddCampfirePage(side: true);
        page.AddContent(outcomeText);

        uiHelper.ShowDismissOnlyChoiceDialog(this, b => b
            .SetTextContent(outcomeText)
            .SetCampfireSideImage()
        );

        Conclude();
    }

    SimpleChoiceData[] CreateChoices(string ch3Key) => [
        new(
            t.TEventChoice(Id, 0),
            t.T($"LV.BCEv.{Id}.C1N.{ch3Key}").Format(GetSafetyTrialNoteFormats(ch3Key))
        ),
        new(
            t.TEventChoice(Id, 1),
            t.TEventChoiceNote(Id, 1).Format(TrialDays, SendAwayResources.ToMixedText())
        ),
        new(
            t.TEventChoice(Id, 2),
            t.TEventChoiceNote(Id, 2)
        ),
    ];

    object[] GetSafetyTrialNoteFormats(string ch3Key) => ch3Key switch
    {
        "Ch3C1" => [TrialDays, TargetWellbeing],
        "Ch3C2" or "Ch3C3" => [TrialDays],
        _ => throw new ArgumentOutOfRangeException(nameof(ch3Key)),
    };

    bool IsSafetyTrialMet(int ch3Choice) => ch3Choice switch
    {
        0 => IsWellbeingTrialMet(),
        1 => true, // Flood successful when no flood events were triggered, so no need for a separate check.
        2 => IsShelterFoodWaterTrialMet(),
        _ => throw new ArgumentOutOfRangeException(nameof(ch3Choice)),
    };

    bool IsWellbeingTrialMet() => wellbeingService.AverageGlobalWellbeing >= TargetWellbeing;

    bool IsShelterFoodWaterTrialMet()
    {
        foreach (var b in beaverPopulation._beaverCollection.Beavers)
        {
            if (!b.GetComponent<Dweller>().HasHome) { return false; }

            var need = b.GetNeedManager();
            if (!MetNeed(need, "Hunger") || !MetNeed(need, "Thirst")) { return false; }
        }

        return true;

        static bool MetNeed(NeedManager man, string id)
        {
            var n = man.GetNeed(id);
            return n.Points >= 0;
        }
    }

    string ApplySuccessBuff()
    {
        var buffDesc = t.T("LV.BCEv.WorkingSpeedDesc", WelcomeBuffAmount);

        characterStatusHelper.AddOrUpdateLimitedTimeBonus(new(
            SuccessBuffId,
            CharacterType.AdultBeaver | CharacterType.ChildBeaver,
            [new(BonusType.WorkingSpeed, WelcomeBuffAmount)],
            t.T($"LV.BCEv.{Id}.BuffName"),
            buffDesc
        ), StatusDays);

        return buffDesc;
    }

    string ApplyFailureDebuff()
    {
        var debuffDesc = t.T("LV.BCEv.WorkingSpeedDesc", FailureDebuffAmount);

        characterStatusHelper.AddOrUpdateLimitedTimeBonus(new(
            FailureDebuffId,
            CharacterType.AdultBeaver,
            [new(BonusType.WorkingSpeed, FailureDebuffAmount)],
            t.T($"LV.BCEv.{Id}.DebuffName"),
            debuffDesc
        ), StatusDays);

        return debuffDesc;
    }

    static string GetChapter3ChoiceKey(int choice) => $"Ch3C{choice + 1}";
}
