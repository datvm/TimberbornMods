
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

    public override HashSet<string> SupportedOneTimeIds { get; } = [.. ModUtils.WaterWheelUpIds, ModUtils.WaterWheelFlowUp1Id, ModUtils.WindmillHeightUpId];
    public override HashSet<string> SupportedDailyIds { get; } = [ModUtils.WaterWheelFlowUp2Id];
    public override IEnumerable<Type> DailyBuffInstanceTypes { get; } = [];

    public override void ProcessDailyBuffs(IEnumerable<ScientificProjectInfo> activeProjects)
    {
        RefreshWaterWheelFlowUpBuff();
    }

    public override void RefreshOnetimeBuffs(ScientificProjectSpec? justUnlocked)
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
