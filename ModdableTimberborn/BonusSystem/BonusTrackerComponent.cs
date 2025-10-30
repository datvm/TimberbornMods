namespace ModdableTimberborn.BonusSystem;

public class BonusTrackerComponent : BaseComponent, IBonusTrackerComponent, IAwakableComponent
{

    public BonusTracker BonusTracker { get; private set; } = null!;

    public void Awake()
    {
        var bm = this.GetBonusManager();
        BonusTracker = new(bm);
    }

}
