namespace ModdableTimberborn.BonusSystem;

public class BonusTrackerComponent : BaseComponent, IBonusTrackerComponent
{

    public BonusTracker BonusTracker { get; private set; } = null!;

    public void Awake()
    {
        var bm = this.GetBonusManager();
        BonusTracker = new(bm);
    }

}
