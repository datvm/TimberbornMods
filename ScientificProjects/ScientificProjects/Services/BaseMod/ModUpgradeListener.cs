namespace ScientificProjects.Services.BaseMod;

public class ModUpgradeListener(
    ScientificProjectUnlockService unlocks,
    ScientificProjectDailyService daily,
    CharacterTracker tracker,
    ScientificProjectService sp
) : ILoadableSingleton
{
    public const string WorkEffBonusId = "SPWorkEffUpgrade";
    public const string MoveSpeedBonusId = "SPMoveSpeedUpgrade";
    public const string CarryBonusId = "SPCarryUpgrade";

    public void Load()
    {
        unlocks.OnProjectUnlocked += OnProjectUnlocked;
        daily.OnDailyPaymentResolved += OnDailyPaymentResolved;
        tracker.OnRegistered += OnNewCharacterRegistered;

        UpdateAllCharacters();
    }

    void OnNewCharacterRegistered(CharacterProjectUpgradeComponent e)
    {
        UpdateCharacters([e]);
    }

    private void OnDailyPaymentResolved()
    {
        UpdateAllCharacters();
    }

    private void OnProjectUnlocked(ScientificProjectSpec e)
    {
        if (e.Id == ScientificProjectsUtils.WorkEffUpgrade1Id)
        {
            UpdateWorkEffUpgrades(tracker.Adults);
        }
        else if (ScientificProjectsUtils.MoveSpeedUpgradeIds.Contains(e.Id))
        {
            UpdateMoveSpeedUpgrades(tracker.AllCharacters);
        }
    }

    void UpdateAllCharacters() => UpdateCharacters(tracker.AllCharacters);
    void UpdateCharacters(IEnumerable<CharacterProjectUpgradeComponent> characters)
    {
        UpdateWorkEffUpgrades(characters);
        UpdateMoveSpeedUpgrades(characters);
        UpdateCarryUpgrades(characters);
    }

    void UpdateWorkEffUpgrades(IEnumerable<CharacterProjectUpgradeComponent> characters)
    {
        var workEff = WorkEffBonus;
        if (workEff == 0) { return; }

        foreach (var c in characters)
        {
            if (c.CharacterType == CharacterType.AdultBeaver)
            {
                c.BonusTracker.AddOrUpdate(new(WorkEffBonusId, BonusType.WorkingSpeed, workEff));
            }
        }
    }

    void UpdateMoveSpeedUpgrades(IEnumerable<CharacterProjectUpgradeComponent> characters)
    {
        var speed = SpeedBonus;
        if (speed == 0) { return; }

        foreach (var c in characters)
        {
            if (c.CharacterType.IsBeaver())
            {
                c.BonusTracker.AddOrUpdate(new(MoveSpeedBonusId, BonusType.MovementSpeed, speed));
            }
        }
    }

    void UpdateCarryUpgrades(IEnumerable<CharacterProjectUpgradeComponent> characters)
    {
        var beaverCarry = BeaverCarryBonus;
        if (beaverCarry == 0) { return; } // Without this upgrade, no one is getting anything

        var builderCarry = BuilderCarryBonus;

        foreach (var c in characters)
        {
            var trackBuilder = builderCarry > 0 && c.Worker.IsBuilder();

            var total =
                +(c.CharacterType.IsBeaver() ? beaverCarry : 0)
                + (trackBuilder ? builderCarry : 0);

            c.BonusTracker.AddOrUpdateOrRemove(new(CarryBonusId, BonusType.CarryingCapacity, total));
        }
    }

    float WorkEffBonus => sp.GetActiveEffects(ScientificProjectsUtils.WorkEffUpgradeIds, 0);
    float SpeedBonus => sp.GetActiveEffects(ScientificProjectsUtils.MoveSpeedUpgradeIds, 0);
    float BeaverCarryBonus => sp.GetActiveEffects([ScientificProjectsUtils.CarryUpgradeId], 0);
    float BuilderCarryBonus => sp.GetActiveEffects([ScientificProjectsUtils.CarryBuilderUpgradeId], 0);

}
