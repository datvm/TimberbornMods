global using ScientificProjects.Buffs;

namespace BrainPowerSPs.Buffs;

public class WaterWheelBuff(
    ISingletonLoader loader,
    IBuffService buffs,
    EventBus eb,
    ScientificProjectService projects
) : CommonProjectsBuff(loader, buffs, eb, projects)
{
    static readonly SingletonKey SaveKey = new("WaterWheelBuff");

    protected override SingletonKey SingletonKey { get; } = SaveKey;

    protected override HashSet<string> SupportedOneTimeIds { get; } = [.. ModUtils.WaterWheelUpIds, ModUtils.WaterWheelFlowUp1Id];
    protected override HashSet<string> SupportedDailyIds { get; } = [ModUtils.WaterWheelFlowUp2Id];
    protected override IEnumerable<Type> DailyBuffInstanceTypes { get; } = [];

    protected override void ProcessDailyBuffs(IEnumerable<ScientificProjectInfo> activeProjects)
    {
        RefreshWaterWheelFlowUpBuff();
    }

    protected override void RefreshOnetimeBuffs(ScientificProjectSpec? justUnlocked)
    {
        if (justUnlocked is null || ModUtils.WaterWheelUpIds.Contains(justUnlocked.Id))
        {
            RefreshWaterWheelUpBuffs();
        }

        if (justUnlocked is null || justUnlocked.Id == ModUtils.WaterWheelFlowUp1Id)
        {
            RefreshWaterWheelFlowUpBuff();
        }
    }

    void RefreshWaterWheelUpBuffs()
    {
        RemoveBuffInstances<WaterWheelUpBuffInst>();

        var unlocked = GetUnlockedOrActiveProjects(ModUtils.WaterWheelUpIds);
        if (unlocked.Count == 0) { return; }

        var instance = buffs.CreateBuffInstance<WaterWheelBuff, WaterWheelUpBuffInst, IEnumerable<ScientificProjectInfo>>(this, unlocked);
        buffs.Apply(instance);
    }

    void RefreshWaterWheelFlowUpBuff()
    {
        RemoveBuffInstances<WaterWheelFlowUpBuffInst>();

        var unlocked = GetUnlockedOrActiveProjects(ModUtils.WaterWheelFlowUpIds);
        if (unlocked.Count == 0) { return; }

        var instance = buffs.CreateBuffInstance<WaterWheelBuff, WaterWheelFlowUpBuffInst, IEnumerable<ScientificProjectInfo>>(this, unlocked);
        buffs.Apply(instance);
    }

}
