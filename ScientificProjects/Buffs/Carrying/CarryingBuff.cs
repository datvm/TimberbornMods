
namespace ScientificProjects.Buffs;

/// <summary>
/// This is a special buff where there are two buff instances created: one for everyone and one for builder targets only.
/// </summary>
public sealed class CarryingBuff(
    ISingletonLoader loader,
    IBuffService buffs,
    ScientificProjectService projects,
    EventBus eb,
    ILoc t
) : CommonStepProjectBuff<CarryingBuff, CarryingBuffInst>(loader, buffs, projects, eb)
{
    static readonly SingletonKey SaveKey = new("CarryingBuff");
    static readonly ImmutableHashSet<string> ProjectIdsValues = ["CarryUpgrade1"];
    static readonly ImmutableHashSet<string> StepProjectIdsValues = ["CarryBuilderUpgrade"];

    protected override SingletonKey SingletonKey => SaveKey;

    protected override ImmutableHashSet<string> StepProjectIds => StepProjectIdsValues;
    protected override ImmutableHashSet<string> ProjectIds => ProjectIdsValues;

    public override string Name { get; protected set; }  = "LV.SP.CarryUpgradeBuff".T(t);
    public override string Description { get; protected set; } = "LV.SP.CarryUpgradeBuffDesc".T(t);

    protected override void RefreshBuff()
    {
        // Just do the work from scratch, don't call base
        RemoveExistingBuff<CarryingBuffInst>();
        RemoveExistingBuff<CarryingBuilderBuffInst>();

        var relevantProjs = GetRelevantProjects();
        if (!relevantProjs.Any()) { return; }

        var generalBuff = CreateInstance(relevantProjs);
        buffs.Apply(generalBuff);

        var builderBuff = CreateBuilderInstance(relevantProjs);
        buffs.Apply(builderBuff);
    }

    CarryingBuilderBuffInst CreateBuilderInstance(IEnumerable<ScientificProjectInfo> relevantProjs)
    {
        return buffs.CreateBuffInstance<CarryingBuff, CarryingBuilderBuffInst, IEnumerable<ScientificProjectInfo>>(
            this,
            relevantProjs
        );
    }

}
