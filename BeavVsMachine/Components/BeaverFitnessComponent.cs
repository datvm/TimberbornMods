namespace BeavVsMachine.Components;

public class BeaverFitnessComponent : BaseComponent, IEntityEffectDescriber
{
    public const string FitnessId = "Fitness";
    public const string FitnessBonusId = "FitnessBonus";

    const int MinimumOusideHours = 4;
    const int HoursPerPositivePoint = 1;

#nullable disable
    NeedManager needManager;
    BonusTrackerComponent bonusTracker;
#nullable enable

    static readonly InstantEffect GoodEffect = new(FitnessId, 1f, 1);
    static readonly InstantEffect BadEffect = new(FitnessId, -1f, 1);

    int currFitness = 0;
    int currBonus = 0;
    float todayOutside = MinimumOusideHours;

    public int Order { get; }
    public int FitnessPoints => Mathf.RoundToInt(needManager.GetNeedPoints(FitnessId));

    public void Start()
    {
        needManager = GetComponentFast<NeedManager>();
        bonusTracker = this.GetBonusTracker();

        UpdateFitnessBonus();
    }

    public void AddTodayFitness(float time) => todayOutside += time;

    public void UpdateDayFitness()
    {
        if (todayOutside < MinimumOusideHours)
        {
            needManager.ApplyEffect(BadEffect);
        }
        else
        {
            var currPoint = FitnessPoints;

            if (currPoint < 0)
            {
                needManager.ApplyEffect(GoodEffect);
            }
            else
            {
                var maintainingPoint = currPoint * HoursPerPositivePoint + MinimumOusideHours;

                if (todayOutside >= maintainingPoint + HoursPerPositivePoint)
                {
                    needManager.ApplyEffect(GoodEffect);
                }
                else if (todayOutside < maintainingPoint)
                {
                    needManager.ApplyEffect(BadEffect);
                }
            }
        }
        todayOutside = 0;

        UpdateFitnessBonus();
    }

    void UpdateFitnessBonus()
    {
        var need = needManager.GetNeedPoints(FitnessId);
        currFitness = Mathf.RoundToInt(need);
        currBonus = Math.Max(0, currFitness);

        bonusTracker.AddOrUpdate(new(FitnessBonusId, [
            BonusType.MovementSpeed.ToBonusSpec(currBonus),
            BonusType.CarryingCapacity.ToBonusSpec(currBonus),
        ]));
    }

    public EntityEffectDescription? Describe(ILoc t, IDayNightCycle dayNightCycle)
        => new(
            t.T("LV.BVM.FitnessTitle", currFitness),
            t.T("LV.BVM.FitnessDesc", currBonus, todayOutside));

}
