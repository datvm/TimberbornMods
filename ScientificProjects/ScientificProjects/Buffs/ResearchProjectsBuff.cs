using ScientificProjects.Extensions;

namespace ScientificProjects.Buffs;

public class ResearchProjectsBuff(
    ISingletonLoader loader,
    IBuffService buffs,
    EventBus eb,
    ScientificProjectService projects
) : CommonProjectsBuff(loader, buffs, eb, projects)
{
    static readonly SingletonKey SaveKey = new("ResearchProjectsBuff");
    protected override SingletonKey SingletonKey => SaveKey;

    public const string FolktailsFactionUpgrade = "FtPlankUpgrade";
    public const string IronTeethFactionUpgrade = "ItSmelterUpgrade";

    public static readonly ImmutableHashSet<string> MoveSpeedUpgrades = ["MoveSpeedUp1", "MoveSpeedUp2", "MoveSpeedUp3"];
    public static readonly ImmutableHashSet<string> OnceCarryUpgrades = ["CarryUpgrade1"];
    public static readonly ImmutableHashSet<string> OnceWorkEffUpgrades = ["WorkEffUpgrade1"];
    public static readonly ImmutableHashSet<string> OnceFactionUpgrades = [FolktailsFactionUpgrade, IronTeethFactionUpgrade];

    public static readonly ImmutableHashSet<string> CarryBuilderUpgrade = ["CarryBuilderUpgrade"];
    public static readonly ImmutableHashSet<string> WorkEffDailyUpgrade = ["WorkEffUpgrade2"];

    public static readonly ImmutableArray<string> AllWorkEffUpgrades = [.. OnceWorkEffUpgrades, .. WorkEffDailyUpgrade];

    protected override HashSet<string> SupportedOneTimeIds { get; } = [.. MoveSpeedUpgrades, .. OnceCarryUpgrades, .. OnceWorkEffUpgrades, .. OnceFactionUpgrades];
    protected override HashSet<string> SupportedDailyIds { get; } = [.. CarryBuilderUpgrade, .. WorkEffDailyUpgrade];

    protected override IEnumerable<Type> DailyBuffInstanceTypes { get; } = [];

    protected override void ProcessDailyBuffs(IEnumerable<ScientificProjectInfo> activeProjects)
    {
        ProcessWorkEffBuffs();
        ProcessCarryBuilderBuffs();
    }

    protected override void RefreshOnetimeBuffs(ScientificProjectSpec? justUnlocked)
    {
        if (justUnlocked is null || MoveSpeedUpgrades.Contains(justUnlocked.Id))
        {
            ProcessMovementBuffs();
        }

        if (justUnlocked is null || OnceWorkEffUpgrades.Contains(justUnlocked.Id))
        {
            ProcessWorkEffBuffs();
        }

        if (justUnlocked is null || OnceCarryUpgrades.Contains(justUnlocked.Id))
        {
            ProcessCarryBuffs();
        }

        if (justUnlocked is null || OnceFactionUpgrades.Contains(justUnlocked.Id))
        {
            ProcessFactionBuffs();
        }
    }

    void ProcessFactionBuffs()
    {
        RemoveBuffInstances<FactionUpgradeBuffInst>();

        var projs = GetUnlockedOrActiveProjects(OnceFactionUpgrades);
        if (projs.Count == 0) { return; }

        this.CreateInstance(projs, out FactionUpgradeBuffInst instant);
        buffs.Apply(instant);
    }

    void ProcessMovementBuffs()
    {
        RemoveBuffInstances<MoveSpeedUpgradeBuffInst>();

        var projs = GetUnlockedOrActiveProjects(MoveSpeedUpgrades);
        if (projs.Count == 0) { return; }

        this.CreateInstance(projs, out MoveSpeedUpgradeBuffInst instant);
        buffs.Apply(instant);
    }

    void ProcessWorkEffBuffs()
    {
        RemoveBuffInstances<WorkEffUpgradeBuffInst>();

        var projs = GetUnlockedOrActiveProjects(AllWorkEffUpgrades);
        if (projs.Count == 0) { return; }

        this.CreateInstance(projs, out WorkEffUpgradeBuffInst instant);
        buffs.Apply(instant);
    }

    void ProcessCarryBuffs()
    {
        RemoveBuffInstances<CarryingUpgradeBuffInst>();

        var projs = GetUnlockedOrActiveProjects(OnceCarryUpgrades);
        if (projs.Count == 0) { return; }

        this.CreateInstance(projs, out CarryingUpgradeBuffInst inst);
        buffs.Apply(inst);
    }

    void ProcessCarryBuilderBuffs()
    {
        RemoveBuffInstances<CarryingBuilderUpgradeBuffInst>();

        var projs = GetUnlockedOrActiveProjects(CarryBuilderUpgrade);
        if (projs.Count == 0) { return; }

        this.CreateInstance(projs, out CarryingBuilderUpgradeBuffInst inst);
        buffs.Apply(inst);
    }

}
