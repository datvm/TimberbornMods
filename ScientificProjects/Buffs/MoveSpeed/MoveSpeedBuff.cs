namespace ScientificProjects.Buffs;

public class MoveSpeedBuff(
    ISingletonLoader loader,
    IBuffService buffs,
    ILoc t,
    ScientificProjectService projects,
    EventBus eb
) : CommonProjectBuff<MoveSpeedBuff, MoveSpeedBuffInst>(loader, buffs, projects, eb)
{
    static readonly SingletonKey SaveKey = new("MoveSpeedBuff");
    static readonly ImmutableHashSet<string> ProjectIdsValues = ["MoveSpeedUp1", "MoveSpeedUp2", "MoveSpeedUp3"];

    protected override SingletonKey SingletonKey => SaveKey;
    protected override ImmutableHashSet<string> ProjectIds => ProjectIdsValues;

    public override string Name { get; protected set; } = "LV.SP.MoveSpeedUpgradeBuff".T(t);
    public override string Description { get; protected set; } = "LV.SP.MoveSpeedUpgradeBuffDesc".T(t);

}
