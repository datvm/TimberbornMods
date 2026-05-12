namespace BeaverChronicles.Events.Implementations;

public class Unhoused(
    ChronicleEventUIHelper uiHelper,
    ILoc t,
    ActiveChronicleEventService activeService,
    CharacterStatusHelper characterStatusHelper,
    IDayNightCycle dayNightCycle,
    PopulationHelper populationHelper
) : SavableChronicleEventBase(activeService)
{
    const int MinDay = 5;
    const int MaxDay = 30;
    const float MostShelteredRatio = .75f;
    const float MostShelteredDays = 3f;
    const float AllShelteredDays = 5f;
    const float BuffDays = 5f;
    const float DebuffDays = 3f;
    const float BuffAmount = .075f;
    const float StrongBuffAmount = .10f;
    const float DebuffAmount = -.15f;

    const string BuffId = nameof(Unhoused) + "_Sheltered";
    const string DebuffId = nameof(Unhoused) + "_Unhoused";

    static readonly object[][] ChoiceFormats = [[MostShelteredDays, MostShelteredRatio], [AllShelteredDays], []];

    public override IReadOnlyCollection<EventTriggerSource> TriggerSources { get; } = [EventTriggerSource.NewDay];

    public override int GetTriggerWeight(IEventTriggerParameters parameters, ChronicleEventService chronicleEventService)
    {
        var day = dayNightCycle.DayNumber;
        if (day > MaxDay || AreAllBeaversSheltered())
        {
            return -1;
        }

        return day >= MinDay ? 200 : 0;
    }

    void RegisterPromise(int index, string description)
    {
        activeService.SetActiveDescription(description);
        activeService.RegisterTimeLimit((float)ChoiceFormats[index][0], OnPromiseDue);
    }

    void OnPromiseDue()
    {
        if (historyRecord is null || historyRecord.Id != Id)
        {
            return;
        }

        if (!historyRecord.TryGetChoice(0, out var index))
        {
            throw new InvalidOperationException("The unhoused event has no recorded promise choice.");
        }

        if (IsPromiseMet(index))
        {
            OnPromiseKept(index);
        }
        else
        {
            OnPromiseFailed();
        }
    }

    void OnPromiseKept(int index)
    {
        var buffAmount = index == 0 ? BuffAmount : StrongBuffAmount;
        var buffDesc = t.T($"LV.BCEv.Unhoused.Buff{index + 1}Desc", buffAmount);

        LimitedTimeCharacterStatus status = index switch
        {
            0 => CreateBuff([new(BonusType.WorkingSpeed, buffAmount)], buffDesc),
            1 => CreateBuff([new(BonusType.WorkingSpeed, buffAmount), new(BonusType.MovementSpeed, buffAmount)], buffDesc),
            _ => throw new ArgumentOutOfRangeException(),
        };
        characterStatusHelper.AddOrUpdateLimitedTimeBonus(status, BuffDays);

        var content = t.TEventOutcome(Id, index).Format(buffDesc, BuffDays);
        var imgPath = ChronicleEventUIHelper.GetTopImagePath(Id + "_O1");
        var page = historyRecord!.AddPage(topImagePath: imgPath);
        page.AddContent(content);
        Conclude();

        uiHelper.ShowDismissOnlyChoiceDialog(this, b => b
            .SetTextContent(content)
            .SetTopImage(imgPath)
        );

        LimitedTimeCharacterStatus CreateBuff(BonusStat[] bonuses, string desc) => new(
            BuffId, CharacterType.AdultBeaver | CharacterType.ChildBeaver,
            bonuses, t.T("LV.BCEv.Unhoused.BuffName"), desc
        );
    }

    void OnPromiseFailed()
    {
        var debuffDesc = t.T("LV.BCEv.WorkingSpeedDesc", DebuffAmount);

        var status = new LimitedTimeCharacterStatus(
            DebuffId,
            CharacterType.AdultBeaver | CharacterType.ChildBeaver,
            [new(BonusType.WorkingSpeed, DebuffAmount)],
            t.T("LV.BCEv.Unhoused.DebuffName"), debuffDesc);

        characterStatusHelper.AddOrUpdateLimitedTimeBonus(status, DebuffDays);

        var content = t.TEventOutcome(Id, 2).Format(debuffDesc, DebuffDays);
        var page = historyRecord!.AddPage(top: true);
        page.AddContent(content);
        Conclude();

        uiHelper.ShowDismissOnlyChoiceDialog(this, b => b
            .SetTextContent(content)
            .SetTopImage()
        );
    }

    protected override void OnTriggeredFromLoad(int[] choices, IEventTriggerParameters parameters, EventHistoryRecord record)
    {
        if (activeService.SavedProgress is not null)
        {
            var days = (float)ChoiceFormats[choices[0]][0];
            activeService.RegisterSavedTimeLimit(days, OnPromiseDue);
        }
    }

    protected override async void OnNewlyTriggered(IEventTriggerParameters parameters, EventHistoryRecord record)
    {
        var page = historyRecord!.AddPage(top: true);

        var content = t.TEventContent(Id);
        page.AddContent(content);

        var choices = SimpleChoiceData.Create(3, Id, t,
            (i, n) => n.Format(ChoiceFormats[i]),
            i => i == 0 && GetShelteredBeaverRatio() >= MostShelteredRatio
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
                var c = choices[index];
                RegisterPromise(index, c.Text + Environment.NewLine + c.Note);
                break;
            case 2:
                OnPromiseFailed();
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    bool IsPromiseMet(int choiceIndex) => choiceIndex switch
    {
        0 => GetShelteredBeaverRatio() >= MostShelteredRatio,
        1 => AreAllBeaversSheltered(),
        _ => false,
    };

    float GetShelteredBeaverRatio() => populationHelper.GetShelteredRatio();
    bool AreAllBeaversSheltered() => populationHelper.GetShelteredRatio() == 1f;
}
