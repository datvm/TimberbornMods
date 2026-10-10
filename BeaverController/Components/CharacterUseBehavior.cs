namespace BeaverController.Components;

[AddTemplateModule2(typeof(Character))]
public class CharacterUseBehavior(BuildingUseService uses, IDayNightCycle dayNight) : RootBehavior, IAwakableComponent
{
    const float FirstStayHours = 0.5f;
    const float NextStayHours = 0.05f;

    readonly List<ContinuousEffect> effects = [];

    CharacterControllerComponent controller = null!;
    WalkInsideExecutor walk = null!;
    ApplyEffectExecutor stay = null!;
    GoodReserver goods = null!;
    NeedManager needs = null!;
    AttractionAttender attender = null!;

    public void Awake()
    {
        controller = GetComponent<CharacterControllerComponent>();
        walk = GetComponent<WalkInsideExecutor>();
        stay = GetComponent<ApplyEffectExecutor>();
        goods = GetComponent<GoodReserver>();
        needs = GetComponent<NeedManager>();
        attender = GetComponent<AttractionAttender>();

        var manager = GetComponent<BehaviorManager>();
        if (!manager || manager._rootBehaviors.Contains(this)) { return; }

        manager._rootBehaviors.Insert(0, this);
    }

    public override Decision Decide(BehaviorAgent agent)
    {
        if (!controller.HasUse || !walk)
        {
            return Decision.ReleaseNow();
        }

        var door = controller.UseEnterable;
        if (!door || !door.Enabled)
        {
            return Fail(false);
        }

        if (controller.UsesGood)
        {
            return DecideConsume(door);
        }

        return DecideAttraction(door);
    }

    Decision DecideConsume(Enterable door)
    {
        var inventory = controller.UseInventory;
        if (!inventory || !inventory.Enabled || !goods || !needs)
        {
            return Fail(false);
        }

        if (goods.StockReservation.Inventory != inventory)
        {
            if (!uses.TryBestGood(controller, inventory, out _, out var good) || !inventory.HasUnreservedStock(good))
            {
                return Fail(false);
            }

            goods.ReserveExactStockAmount(inventory, good);
        }
        else if (!uses.WorthConsuming(controller, goods.StockReservation.GoodAmount))
        {
            return Fail(false);
        }

        return walk.Launch(door) switch
        {
            ExecutorStatus.Success => Eat(),
            ExecutorStatus.Failure => Fail(true),
            ExecutorStatus.Running => Decision.ReturnWhenFinished(walk),
            _ => throw new ArgumentOutOfRangeException(nameof(walk)),
        };
    }

    Decision DecideAttraction(Enterable door)
    {
        var attraction = door.GetComponent<Attraction>();
        if (!attraction || !attraction.IsUsable || !stay || !attender || !needs)
        {
            return Fail(false);
        }

        return walk.Launch(door) switch
        {
            ExecutorStatus.Success => Stay(attraction),
            ExecutorStatus.Failure => Fail(true),
            ExecutorStatus.Running => Decision.ReturnWhenFinished(walk),
            _ => throw new ArgumentOutOfRangeException(nameof(walk)),
        };
    }

    Decision Stay(Attraction attraction)
    {
        if (uses.StayFinished(attraction, needs, attender.FirstVisit))
        {
            controller.CompleteUse();
            return Decision.ReleaseNow();
        }

        var hours = attender.FirstVisit ? FirstStayHours : NextStayHours;
        attender.FirstVisit = false;
        effects.Clear();
        attraction.GetEfficiencyAdjustedEffects(effects);
        stay.LaunchToTimestamp(effects, dayNight.DayNumberHoursFromNow(hours));
        return Decision.ReturnWhenFinished(stay);
    }

    Decision Eat()
    {
        if (!uses.WorthConsuming(controller, goods.StockReservation.GoodAmount))
        {
            return Fail(false);
        }

        uses.Consume(needs, goods);
        controller.CompleteUse();
        return Decision.ReleaseNow();
    }

    Decision Fail(bool unreachable)
    {
        controller.FailUse(unreachable);
        return Decision.ReleaseNow();
    }
}
