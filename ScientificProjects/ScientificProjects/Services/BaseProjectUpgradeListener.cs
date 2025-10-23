namespace ScientificProjects.Services;

public abstract class BaseProjectUpgradeListener(
    ScientificProjectUnlockService unlocks
) : ILoadableSingleton
{
    protected readonly ScientificProjectUnlockService unlocks = unlocks;

    public virtual void Load()
    {
        unlocks.OnProjectUnlocked += OnProjectUnlocked;
    }

    protected abstract void OnProjectUnlocked(ScientificProjectSpec spec);
}

public abstract class BaseProjectUpgradeWithDailyListener(
    ScientificProjectUnlockService unlocks,
    ScientificProjectDailyService daily
) : BaseProjectUpgradeListener(unlocks)
{
    protected readonly ScientificProjectDailyService daily = daily;

    public override void Load()
    {
        base.Load();
        daily.OnDailyPaymentResolved += OnDailyPaymentResolved;
    }

    protected abstract void OnDailyPaymentResolved();
}
