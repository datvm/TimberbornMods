namespace BuffDebuffDemo.Buffs;

public class PositiveBuff(ISingletonLoader loader, IBuffService buffs, ILoc t) : SimpleFloatBuff<PositiveBuff, PositiveBuffInstance>(loader, buffs)
{
    static readonly SingletonKey SaveKey = new("PositiveBuff");
    protected override SingletonKey SingletonKey => SaveKey;

    public override string Name { get; protected set; } = t.T("LV.BuffDebuffDemo.PositiveBuff");
    public override string Description { get; protected set; } = t.T("LV.BuffDebuffDemo.PositiveBuffDesc");
}
