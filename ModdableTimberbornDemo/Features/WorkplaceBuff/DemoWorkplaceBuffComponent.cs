using ModdableTimberborn.BonusSystem;

namespace ModdableTimberbornDemo.Features.WorkplaceBuff;

public class DemoWorkplaceBuffComponent : TogglableWorkplaceBonusComponent, IEntityEffectDescriber
{
    static readonly BonusTrackerItem DemoBonuses = new("DemoWorkplaceBuff", [
        BonusType.MovementSpeed.ToBonusSpec(1f), // Either use BonusType enum
        new(BonusSystemHelpers.WorkingSpeedId, 1f), // Or constant from BonusSystemHelpers
    ]);

    public int Order { get; }
    protected override BonusTrackerItem Bonuses { get; } = DemoBonuses;

    public EntityEffectDescription? Describe(ILoc t, IDayNightCycle dayNightCycle)
        => Active ? 
            new(
                "Demo Workplace Buff",
                t.T("LV.DemoMT.WorkplaceBuffDesc", DemoBonuses.Bonuses[0].MultiplierDelta, DemoBonuses.Bonuses[1].MultiplierDelta)
            )
            : null;
}
