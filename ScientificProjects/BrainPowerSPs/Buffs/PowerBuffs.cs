global using ScientificProjects.Buffs;

namespace BrainPowerSPs.Buffs;

public class PowerBuffs(
    ISingletonLoader loader,
    IBuffService buffs,
    EventBus eb,
    ScientificProjectService projects
) : CommonProjectsBuff(loader, buffs, eb, projects)
{
    static readonly SingletonKey SaveKey = new("WaterWheelBuff");

    protected override SingletonKey SingletonKey { get; } = SaveKey;

    protected override HashSet<string> SupportedOneTimeIds { get; } = [.. ModUtils.WaterWheelUpIds, ModUtils.WaterWheelFlowUp1Id, ModUtils.WindmillHeightUpId];
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

        if (justUnlocked is null || justUnlocked.Id == ModUtils.WindmillHeightUpId)
        {
            RefreshWindmillBuffs();
        }
    }

    void RefreshWaterWheelUpBuffs()
    {
        RemoveBuffInstances<WaterWheelUpBuffInst>();

        var unlocked = GetUnlockedOrActiveProjects(ModUtils.WaterWheelUpIds);
        if (unlocked.Count == 0) { return; }

        this.CreateInstance(unlocked, out WaterWheelUpBuffInst instance);
        buffs.Apply(instance);
    }

    void RefreshWaterWheelFlowUpBuff()
    {
        RemoveBuffInstances<WaterWheelFlowUpBuffInst>();

        var unlocked = GetUnlockedOrActiveProjects(ModUtils.WaterWheelFlowUpIds);
        if (unlocked.Count == 0) { return; }

        this.CreateInstance(unlocked, out WaterWheelFlowUpBuffInst instance);
        buffs.Apply(instance);
    }

    void RefreshWindmillBuffs()
    {
        RemoveBuffInstances<WindmillBuffInstant>();
        var unlocked = GetUnlockedOrActiveProjects([ModUtils.WindmillHeightUpId]);
        if (unlocked.Count == 0) { return; }

        this.CreateInstance(unlocked, out WindmillBuffInstant instance);
        buffs.Apply(instance);
    }

}
