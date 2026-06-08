namespace BeaverChronicles.Events.Implementations;

public class ContaminationConspiracy(
    ChronicleEventUIHelper uiHelper,
    ILoc t,
    ActiveChronicleEventService activeService,
    CharacterStatusHelper characterStatusHelper,
    EntityRegistry entityRegistry
) : SavableChronicleEventBase(activeService), IPostLoadableSingleton
{
    public const string EventId = nameof(ContaminationConspiracy);
    public override string Id => EventId;

    const float CureDays = 15f;
    const float BuffDays = 5f;
    const float DebuffDays = 5f;
    const float BuffAmount = .15f;
    const float DebuffAmount = -.4f;

    const string BuffId = nameof(ContaminationConspiracy) + "_Reassured";
    const string DebuffId = nameof(ContaminationConspiracy) + "_QuietDread";

    bool shouldCheckForBeaver;

    string CharacterName
    {
        get => GetParameter("CharacterName");
        set => SetParameter("CharacterName", value);
    }

    Guid CharacterEntityId
    {
        get => Guid.Parse(GetParameter("CharacterEntityId"));
        set => SetParameter("CharacterEntityId", value.ToString());
    }

    public override IReadOnlyCollection<EventTriggerSource> TriggerSources => [EventTriggerSource.Contaminated];

    public override int GetTriggerWeight(ChronicleEventContext context)
    {
        var p = context.Parameters.GetParameterOrDefault<CharacterParameters>();
        return p is not null && p.IsBeaver ? 100 : 0;
    }

    protected override void OnTriggeredFromLoad(int[] choices, IEventTriggerParameters parameters, EventHistoryRecord record)
    {
        if (activeService.SavedProgress is not null)
        {
            activeService.RegisterSavedTimeLimit(CureDays, OnTimeDue);
        }

        shouldCheckForBeaver = true; // Do not check now because Beaver entities are not loaded yet, will check in PostLoad.
    }

    protected override async void OnNewlyTriggered(IEventTriggerParameters parameters, EventHistoryRecord record)
    {
        var character = parameters.GetParameter<CharacterParameters>().Character;
        CharacterName = character.FirstName;
        CharacterEntityId = character.GetEntityId();

        var page = record.AddPage(side: true);

        var content = t.TEventContent(Id).Format(CharacterName);
        page.AddContent(content);

        var choices = SimpleChoiceData.Create(1, Id, t,
            (i, n) => n.Format(CharacterName, CureDays)
        );

        var index = await uiHelper.ShowChoiceDialogAsync(this, b => b
            .SetTextContent(content)
            .SetSideImage()
            .AddChoices(choices)
        );

        // No need to record choice, there is no branching.
        var c = choices[index];
        c.Record(page);
        activeService.SetActiveDescription(c.Text + Environment.NewLine + c.Note.UncenterMixed());
        activeService.RegisterTimeLimit(CureDays, OnTimeDue);

        MonitorBeaver(character);
    }

    void MonitorBeaver(Character c)
    {
        if (!c)
        {
            OnBeaverDied(null!, null!);
            return;
        }

        var needMan = c.GetComponent<NeedManager>();
        needMan.NeedChangedActiveState += OnNeedActiveChanged;
        c.Died += OnBeaverDied;
    }

    void OnNeedActiveChanged(object sender, NeedChangedActiveStateEventArgs e)
    {
        if (!Active
            || e.NeedSpec.Id != ChronicleGameEventHandler.ContaminationId
            || e.IsActive) { return; }

        OnBeaverCured();
    }

    void OnBeaverDied(object _, EventArgs _2)
    {
        if (!Active) { return; }
        OnPromiseFailed();
    }

    void CleanupCharacter()
    {
        var character = GetCharacter()!;
        if (!character) { return; }

        character.GetNeedManager().NeedChangedActiveState -= OnNeedActiveChanged;
        character.Died -= OnBeaverDied;
    }

    protected override void OnConcluded()
    {
        base.OnConcluded();

        CleanupCharacter();
    }

    void OnBeaverCured()
    {
        if (!Active) { return; }

        var buffName = t.T("LV.BCEv.ContaminationConspiracy.BuffName");
        var buffDesc = t.T("LV.BCEv.MovementSpeedDesc", BuffAmount) + " " + t.T("LV.BCEv.WorkingSpeedDesc", BuffAmount);

        characterStatusHelper.AddOrUpdateLimitedTimeBonus(new(
            BuffId,
            CharacterType.AdultBeaver | CharacterType.ChildBeaver,
            [
                new(BonusType.WorkingSpeed, BuffAmount),
                new(BonusType.MovementSpeed, BuffAmount)
            ],
            buffName,
            buffDesc
        ), BuffDays);

        var imgPath = ChronicleEventUIHelper.GetTopImagePath(Id + "_O1");
        var content = t.TEventOutcome(Id, 0).Format(CharacterName, buffName, BuffDays);
        var page = historyRecord!.AddPage(topImagePath: imgPath);
        page.AddContent(content);

        Conclude();

        uiHelper.ShowDismissOnlyChoiceDialog(this, b => b
            .SetTextContent(content)
            .SetTopImage(imgPath)
        );
    }

    void OnTimeDue() => OnPromiseFailed();

    void OnPromiseFailed()
    {
        var debuffName = t.T("LV.BCEv.ContaminationConspiracy.DebuffName");
        var debuffDesc = t.T("LV.BCEv.MovementSpeedDesc", DebuffAmount) + " " + t.T("LV.BCEv.WorkingSpeedDesc", DebuffAmount);

        characterStatusHelper.AddOrUpdateLimitedTimeBonus(new(
            DebuffId,
            CharacterType.AdultBeaver | CharacterType.ChildBeaver,
            [
                new(BonusType.WorkingSpeed, DebuffAmount),
                new(BonusType.MovementSpeed, DebuffAmount)
            ],
            debuffName,
            debuffDesc
        ), DebuffDays);

        var imgPath = ChronicleEventUIHelper.GetTopImagePath(Id + "_O2");
        var content = t.TEventOutcome(Id, 1).Format(CharacterName, debuffDesc, DebuffDays);
        var page = historyRecord!.AddPage(topImagePath: imgPath);
        page.AddContent(content);

        Conclude();

        uiHelper.ShowDismissOnlyChoiceDialog(this, b => b
            .SetTextContent(content)
            .SetTopImage(imgPath)
        );
    }

    public void PostLoad()
    {
        if (!shouldCheckForBeaver) { return; }

        var c = GetCharacter();
        if (c)
        {
            MonitorBeaver(c!);
        }
        else
        {
            OnBeaverDied(null!, null!);
        }
    }

    Character? GetCharacter()
    {
        var e = entityRegistry.GetEntity(CharacterEntityId);
        return e ? e.GetComponent<Character>() : null;
    }

}
