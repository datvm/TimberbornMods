namespace ScientificProjects.Buffs;

public class WorkEffBuff(
    ISingletonLoader loader,
    IBuffService buffs,
    ILoc t,
    ScientificProjectService projects,
    EventBus eb
) : CommonStepProjectBuff<WorkEffBuff, WorkEffBuffInst>(loader, buffs, projects, eb)
{
    static readonly SingletonKey SaveKey = new("WorkEffBuff");
    static readonly ImmutableHashSet<string> ProjectIdsValues = ["WorkEffUpgrade1",];
    static readonly ImmutableHashSet<string> StepProjectIdsValues = ["WorkEffUpgrade2",];

    protected override SingletonKey SingletonKey => SaveKey;
    protected override ImmutableHashSet<string> ProjectIds => ProjectIdsValues;
    protected override ImmutableHashSet<string> StepProjectIds => StepProjectIdsValues;


    public override string Name { get; protected set; } = "LV.SP.WorkEffUpgradeBuff".T(t);
    public override string Description { get; protected set; } = "LV.SP.WorkEffUpgradeBuffDesc".T(t);
}
