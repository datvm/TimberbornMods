namespace GlobalWellbeing.Buffs;

public class WellBeingBuff(ILoc t, WellbeingService wellbeings, IBuffService buffs) : SimpleValueBuff<int, WellBeingBuff, WellBeingBuffInstance>, ILoadableSingleton, ITickableSingleton
{
    public override string Name => t.T("LV.GW.WellbeingBuff");
    public override string Description => t.T("LV.GW.WellbeingBuffDesc");

    ImmutableArray<BuffInstance> currentBuffs = [];
    int? prevWellbeing;

    public void Load()
    {
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

        foreach (var b in currentBuffs)
        {
            buffs.Remove(b);
        }

        var adultBuff = new WellBeingBuffInstance(this, wellbeings.AverageGlobalWellbeing, false);
        var childBuff = new WellBeingBuffInstance(this, wellbeings.AverageGlobalWellbeing, true);
        currentBuffs = [adultBuff, childBuff];

        buffs.Apply(adultBuff);
        buffs.Apply(childBuff);
    }

    protected override WellBeingBuffInstance CreateInstance(IBuff buff, int value)
    {
        throw new NotImplementedException("No need for this");
    }

}
