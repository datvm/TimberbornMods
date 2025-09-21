namespace BeavVsMachine.Components;

public class BeaverFitnessComponent : TickableComponent
{
    public const string FitnessId = "Fitness";
    public const string FitnessBonusId = "FitnessBonus";

    const int MaxFitnessBonus = 20;

#nullable disable
    NeedManager needManager;
    Enterer enterer;
    IDayNightCycle dayNightCycle;
    BonusTrackerComponent bonusTracker;
#nullable enable

    static readonly ContinuousEffect OutsideEffect = new(FitnessId, 1f);
    int currBonus = 0;

    [Inject]
    public void Inject(IDayNightCycle dayNightCycle)
    {
        this.dayNightCycle = dayNightCycle;
    }

    public void Awake()
    {
        needManager = GetComponentFast<NeedManager>();
        enterer = GetComponentFast<Enterer>();
        bonusTracker = this.GetBonusTracker();
    }

    public override void Tick()
    {
        if (!enterer.IsInside)
        {
            var time = dayNightCycle.FixedDeltaTimeInHours;
            needManager.ApplyEffect(OutsideEffect, time);
        }

        var need = needManager.GetNeed(FitnessId);
        var p = need.Points;
        var bonus = 0;
        if (p > 0)
        {
            var max = need.NeedSpec.MaximumValue;

            // As p go from 0 to max, bonus goes from 0 to MaxFitnessBonus
            bonus = Mathf.FloorToInt(MaxFitnessBonus * (p / max));
        }

        if (bonus == currBonus) { return; }
        currBonus = bonus;

        if (bonus == 0)
        {
            bonusTracker.Remove(FitnessBonusId);
        }
        else
        {
            bonusTracker.AddOrUpdate(new(FitnessBonusId, [
                BonusType.MovementSpeed.ToBonusSpec(bonus),
                BonusType.CarryingCapacity.ToBonusSpec(bonus)
            ]));
        }
    }

}
