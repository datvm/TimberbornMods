using Unity.Collections;

namespace GlobalWellbeing.Buffs;

public class WellBeingBuff(ILoc t, WellbeingService wellbeings, IBuffService buffs, ISingletonLoader loader) 
    : SimpleValueBuff<WellBeingBuffInstanceData, WellBeingBuff, WellBeingBuffInstance>(loader, buffs),
    ITickableSingleton
{
    static readonly SingletonKey SaveKey = new("WellBeingBuff");

    readonly IBuffService buffs = buffs;

    string name = "";
    public override string Name => name;
    public override string Description => t.T("LV.GW.WellbeingBuffDesc");

    protected override SingletonKey SingletonKey => SaveKey;

    ImmutableArray<BuffInstance> currentBuffs = [];
    int? prevWellbeing;

    protected override void AfterLoad()
    {
        base.AfterLoad();
        UpdateBuffs();
    }

    public void Tick()
    {
        UpdateBuffs();
    }

    public void UpdateBuffs()
    {
        var wellbeing = wellbeings.AverageGlobalWellbeing;
        if (wellbeing == prevWellbeing) { return; }

        prevWellbeing = wellbeing;
        name = t.T("LV.GW.WellbeingBuff", wellbeing);

        foreach (var b in currentBuffs)
        {
            buffs.Remove(b);
        }

        var adultBuff = CreateInstance(new(wellbeing, false));
        var childBuff = CreateInstance(new(wellbeing, true));

        currentBuffs = [adultBuff, childBuff];

        buffs.Apply(adultBuff);
        buffs.Apply(childBuff);
    }

}
