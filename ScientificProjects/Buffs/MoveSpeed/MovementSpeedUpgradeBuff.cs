namespace ScientificProjects.Buffs;

public class MovementSpeedUpgradeBuff(
    ISingletonLoader loader,
    IBuffService buffs,
    ILoc t,
    ScientificProjectService projects,
    EventBus eb
) : SimpleValueBuff<IEnumerable<ScientificProjectSpec>, MovementSpeedUpgradeBuff, MovementSpeedUpgradeBuffInstance>(loader, buffs), IUnloadableSingleton
{
    static readonly SingletonKey SaveKey = new("MovementSpeedUpgradeBuff");
    static readonly ImmutableHashSet<string> ProjectIds = ["MoveSpeedUp1", "MoveSpeedUp2", "MoveSpeedUp3"];

    readonly IBuffService buffs = buffs;

    protected override SingletonKey SingletonKey => SaveKey;

    public override string Name { get; protected set; } = "LV.SP.MoveSpeedUpgradeBuff".T(t);
    public override string Description { get; protected set; } = "LV.SP.MoveSpeedUpgradeBuffDesc".T(t);

    protected override void AfterLoad()
    {
        RefreshBuff();
        eb.Register(this);
    }

    void RefreshBuff()
    {
        var projectList = ProjectIds.Select(projects.GetProject)
            .OrderBy(q => q.Spec.Order);

        var unlockedList = projectList
            .Where(q => q.Unlocked)
            .Select(q => q.Spec)
            .ToImmutableArray();

        // Remove existing buff
        foreach (var i in buffs.GetInstances<MovementSpeedUpgradeBuffInstance>())
        {
            buffs.Remove(i);
        }

        if (unlockedList.Length == 0) { return; }

        var instance = CreateInstance(unlockedList);
        buffs.Apply(instance);
    }

    [OnEvent]
    public void OnProjectUnlocked(OnScientificProjectUnlockedEvent ev)
    {
        if (!ProjectIds.Contains(ev.Project.Id)) { return; }

        RefreshBuff();
    }

    public void Unload()
    {
        eb.Unregister(this);
    }
}
