namespace ScientificProjects.Buffs;

public class ManufactoryEntityTracker : ITrackingEntities
{
    public IEnumerable<Type> TrackingTypes { get; } = [typeof(Manufactory)];
}

public class FactionUpgradeBuffInst : CommonProjectBuffInstance<ResearchProjectsBuff>
{
    
    EntityManager entities = null!;

    public LessPowerBuffEffect LessPowerBuffEffect { get; private set; } = null!;
    public OutputBuffEffect OutputBuffEffect { get; private set; } = null!;

    [Inject]
    public void Inject(EntityManager entities)
    {
        this.entities = entities;
    }

    protected override string GetBuffName(ILoc t) => "LV.SP.FtPlankUpgrade".T(t);
    protected override string GetBuffDescription(ILoc t) => "LV.SP.FtPlankUpgradeLore".T(t);

    public override void Init()
    {
        base.Init();

        var spec = Value.First().Spec;
        OutputBuffEffect = new(
            spec.Parameters[0],
            t);
        LessPowerBuffEffect = new(
            spec.Parameters[1],
            t);

        Effects = [
            LessPowerBuffEffect,
            OutputBuffEffect,
        ];
    }

    protected override IBuffTarget[] CreateTargets() => [..Value.Select(
        q => new FactionUpgradeWorkplaceBuffTarget(
            q.Spec.Id == ResearchProjectsBuff.FolktailsFactionUpgrade,
            entities,
            ev))];
    protected override IBuffEffect? CreateBuffEffect(ScientificProjectInfo info) => null;

}

public class FactionUpgradeWorkplaceBuffTarget(bool isFolktail, EntityManager entities, EventBus eb) : EntityBasedBuffTarget(eb)
{
    public const string WoodWorkshopPrefab = "WoodWorkshop.Folktails";
    public const string SmelterPrefab = "Smelter.IronTeeth";

    protected override bool Filter(EntityComponent entity)
    {
        var pref = entity.GetComponentFast<PrefabSpec>();
        if (pref is null) { return false; }

        return (isFolktail && pref.IsNamed(WoodWorkshopPrefab))
            || (!isFolktail && pref.IsNamed(SmelterPrefab));
    }

    protected override HashSet<BuffableComponent> GetTargets() =>
        [..entities.Get<Manufactory>().AsEnumerable()
            .Where(q => Filter(q.GetComponentFast<EntityComponent>()))
            .Select(q => q.GetBuffable())];

}

public class LessPowerBuffEffect(float value, ILoc t) : SimpleValueBuffEffect<float>(value)
{
    protected override string? GetDescription(float value) => string.Format("LV.SP.BuffLessPowerEff".T(t), value);
}

public class OutputBuffEffect(float value, ILoc t) : SimpleValueBuffEffect<float>(value)
{
    protected override string? GetDescription(float value) => "LV.SP.BuffIncreasedOutput".T(t, value);
}