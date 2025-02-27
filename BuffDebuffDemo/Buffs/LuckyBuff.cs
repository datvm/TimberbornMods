namespace BuffDebuffDemo.Buffs;

// This buff will create LuckyBuffInstance, a timed buff.
// Since LuckyBuffInstance is not a BuffInstance<TValue, TBuff>, we cannot use SimpleValueBuff but only SimpleBuff
// We then make our own `CreateInstance` that was provided by SimpleValueBuff.
public class LuckyBuff(ISingletonLoader loader, IBuffService buffs, ILoc t, EventBus eb) : SimpleBuff(loader, buffs), IUnloadableSingleton
{
    readonly IBuffService buffs = buffs;
    static readonly SingletonKey SaveKey = new("LuckyBuff");
    protected override SingletonKey SingletonKey => SaveKey;

    public override string Name { get; protected set; } = t.T("LV.BuffDebuffDemo.LuckyBuff");
    public override string Description { get; protected set; } = t.T("LV.BuffDebuffDemo.LuckyBuffDesc");

    protected override void AfterLoad()
    {
        base.AfterLoad();
        eb.Register(this);
    }

    public void Unload()
    {
        eb.Unregister(this);
    }

    [OnEvent]
    public void OnDaytimeStart(DaytimeStartEvent _)
    {
        const float MaxBuffPerc = .2f; // Only 20% compared to other buffs' 100%
        const float MaxBuffHours = 30f; // There is a chance two or more buffs exist at once

        // We do not remove existing instance like the other buffs

        // Now we create a random boost for a random amount of hours
        var hours = UnityEngine.Random.Range(0, MaxBuffHours);
        var perc = UnityEngine.Random.Range(0, MaxBuffPerc);

        // Lucky buff is applied with positive percentage
        var instance = CreateInstance(hours, perc);
        buffs.Apply(instance);

        Debug.Log($"Lucky buff applied with {perc:P} speed increased for {hours:F1} hours");
    }

    LuckyBuffInstance CreateInstance(float hours, float perc)
    {
        // Do not just create an instance directly, you need to inject the services
        // And values if it's of IValuedBuffInstance
        return buffs.CreateBuffInstance<LuckyBuff, LuckyBuffInstance, LuckyBuffInstanceValue>(this, new(hours, perc));
    }

}
