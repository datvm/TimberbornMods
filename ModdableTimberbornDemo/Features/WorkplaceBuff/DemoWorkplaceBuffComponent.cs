
namespace ModdableTimberbornDemo.Features.WorkplaceBuff;

public class DemoWorkplaceBuffComponent : TogglableWorkplaceBonusComponent, IEntityEffectDescriber
{
    static readonly IReadOnlyList<BonusSpec> DemoBonuses = [
        new(BonusType.MovementSpeed.ToString(), 1f),
        new(BonusType.WorkingSpeed.ToString(), 1f),
    ];

    public int Order { get; }
    protected override IReadOnlyList<BonusSpec> Bonuses { get; } = DemoBonuses;

    public EntityEffectDescription? Describe(ILoc t, IDayNightCycle dayNightCycle)
        => Active ? 
            new(
                "Demo Workplace Buff",
                t.T("LV.DemoMT.WorkplaceBuffDesc", DemoBonuses[0].MultiplierDelta, DemoBonuses[1].MultiplierDelta)
            )
            : null;
}
