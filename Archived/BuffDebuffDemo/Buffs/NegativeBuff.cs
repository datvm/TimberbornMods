namespace BuffDebuffDemo.Buffs;

// See guide at PositiveBuff.cs
public class NegativeBuff(ISingletonLoader loader, IBuffService buffs, ILoc t, EventBus eb) : SimpleFloatBuff<NegativeBuff, NegativeBuffInstance>(loader, buffs), IUnloadableSingleton
{
    readonly IBuffService buffs = buffs;

    static readonly SingletonKey SaveKey = new("NegativeBuff");
    protected override SingletonKey SingletonKey => SaveKey;

    public override string Name { get; protected set; } = t.T("LV.BuffDebuffDemo.NegativeBuff");
    public override string Description { get; protected set; } = t.T("LV.BuffDebuffDemo.NegativeBuffDesc");

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
        const float MaxDebuffPerc = 1f;

        var existing = buffs.GetInstances<NegativeBuffInstance>();
        foreach (var i in existing)
        {
            buffs.Remove(i);
        }

        var perc = UnityEngine.Random.Range(0, MaxDebuffPerc);

        // Negative buff is applied with negative percentage
        var instance = CreateInstance(-perc);
        buffs.Apply(instance);

        Debug.Log($"Negative buff applied with {perc:P} speed reduction");
    }
}
