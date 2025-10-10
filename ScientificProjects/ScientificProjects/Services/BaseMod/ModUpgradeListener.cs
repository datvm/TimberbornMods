namespace ScientificProjects.Services.BaseMod;

public class ModUpgradeListener(
    ScientificProjectUnlockService unlocks,
    ScientificProjectDailyService daily,
    CharacterTracker tracker,
    WorkplaceTracker wpTracker,
    ScientificProjectService sp
) : ILoadableSingleton
{
    public const string WorkEffBonusId = "SPWorkEffUpgrade";
    public const string MoveSpeedBonusId = "SPMoveSpeedUpgrade";
    public const string CarryBonusId = "SPCarryUpgrade";

    public static bool WheelbarrowUnlocked { get; private set; }

    bool builderBonusActive;

    public void Load()
    {
        WheelbarrowUnlocked = unlocks.IsUnlocked(ScientificProjectsUtils.WheelbarrowsUpgradeId);

        unlocks.OnProjectUnlocked += OnProjectUnlocked;
        daily.OnDailyPaymentResolved += OnDailyPaymentResolved;
        tracker.OnEntityRegistered += OnNewCharacterRegistered;
        wpTracker.OnWorkerAssigned += OnWorkerChanged;
        wpTracker.OnWorkerUnassigned += OnWorkerChanged;

        UpdateAllCharacters();
    }

    private void OnWorkerChanged(WorkplaceTrackerComponent wp, Worker wk)
    {
        if (builderBonusActive && wp.IsBuilderWorkplace)
        {
            UpdateCharacters([wk.GetComponentFast<CharacterTrackerComponent>()]);
        }
    }

    void OnNewCharacterRegistered(CharacterTrackerComponent e)
    {
        UpdateCharacters([e]);
    }

    private void OnDailyPaymentResolved()
    {
        UpdateAllCharacters();
    }

    private void OnProjectUnlocked(ScientificProjectSpec e)
    {
        switch (e.Id)
        {
            case ScientificProjectsUtils.WorkEffUpgrade1Id:
                UpdateWorkEffUpgrades(tracker.Adults);
                break;
            case ScientificProjectsUtils.CarryUpgradeId:
                UpdateCarryUpgrades(tracker.Beavers);
                break;
            case ScientificProjectsUtils.WheelbarrowsUpgradeId:
                WheelbarrowUnlocked = true;
                break;
            default:
                if (ScientificProjectsUtils.MoveSpeedUpgradeIds.Contains(e.Id))
                {
                    UpdateMoveSpeedUpgrades(tracker.Entities);
                }

                break;
        }
    }

    void UpdateAllCharacters() => UpdateCharacters(tracker.Entities);
    void UpdateCharacters(IEnumerable<CharacterTrackerComponent> characters)
    {
        UpdateWorkEffUpgrades(characters);
        UpdateMoveSpeedUpgrades(characters);
        UpdateCarryUpgrades(characters);
    }

    void UpdateWorkEffUpgrades(IEnumerable<CharacterTrackerComponent> characters)
    {
        var workEff = WorkEffBonus;
        if (workEff == 0) { return; }

        foreach (var c in characters)
        {
            if (c.IsAdult)
            {
                c.BonusTracker!.AddOrUpdate(new(WorkEffBonusId, BonusType.WorkingSpeed, workEff));
            }
        }
    }

    void UpdateMoveSpeedUpgrades(IEnumerable<CharacterTrackerComponent> characters)
    {
        var speed = SpeedBonus;
        if (speed == 0) { return; }

        foreach (var c in characters)
        {
            if (c.IsBeaver)
            {
                c.BonusTracker!.AddOrUpdate(new(MoveSpeedBonusId, BonusType.MovementSpeed, speed));
            }
        }
    }

    void UpdateCarryUpgrades(IEnumerable<CharacterTrackerComponent> characters)
    {
        var beaverCarry = BeaverCarryBonus;
        if (beaverCarry == 0) { return; } // Without this upgrade, no one is getting anything

        var builderCarry = BuilderCarryBonus;

        foreach (var c in characters)
        {
            var trackBuilder = builderCarry > 0 && c.Worker.IsBuilder();

            var total =
                +(c.IsBeaver ? beaverCarry : 0)
                + (trackBuilder ? builderCarry : 0);

            c.BonusTracker!.AddOrUpdateOrRemove(new(CarryBonusId, BonusType.CarryingCapacity, total));
        }
    }

    float WorkEffBonus => sp.GetActiveEffects(ScientificProjectsUtils.WorkEffUpgradeIds, 0);
    float SpeedBonus => sp.GetActiveEffects(ScientificProjectsUtils.MoveSpeedUpgradeIds, 0);
    float BeaverCarryBonus => sp.GetActiveEffects([ScientificProjectsUtils.CarryUpgradeId], 0);
    float BuilderCarryBonus
    {
        get
        {
            var eff = sp.GetActiveEffects([ScientificProjectsUtils.CarryBuilderUpgradeId], 0);
            builderBonusActive = eff > 0;
            return eff;
        }
    }
}
