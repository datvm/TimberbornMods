namespace BuildingRenovations.Components.Bonuses;

[AddTemplateModule2(typeof(Workplace))]
public class WorkplaceBonusComponent : BaseComponent, IAwakableComponent, IEntityMultiEffectsDescriber
{

    Workplace workplace = null!;
    readonly Dictionary<string, WorkerBonus> activeBonuses = [];

    public void Awake()
    {
        workplace = GetComponent<Workplace>();
        workplace.WorkerAssigned += OnWorkerAssigned;
        workplace.WorkerUnassigned += OnWorkerUnassigned;
    }

    public void AddBonus(WorkerBonus bonus)
    {
        RemoveBonus(bonus.Id);

        activeBonuses[bonus.Id] = bonus;
        ApplyToAssignedWorkers(bonus, add: true);
    }

    public void RemoveBonus(string id)
    {
        if (!activeBonuses.Remove(id, out var bonus)) { return; }

        ApplyToAssignedWorkers(bonus, add: false);
    }

    void OnWorkerAssigned(object sender, WorkerChangedEventArgs e)
        => ApplyAll(e.Worker, add: true);

    void OnWorkerUnassigned(object sender, WorkerChangedEventArgs e)
        => ApplyAll(e.Worker, add: false);

    void ApplyAll(Worker worker, bool add)
    {
        foreach (var bonus in activeBonuses.Values)
        {
            Apply(worker, bonus, add);
        }
    }

    void ApplyToAssignedWorkers(WorkerBonus bonus, bool add)
    {
        foreach (var worker in workplace.AssignedWorkers)
        {
            Apply(worker, bonus, add);
        }
    }

    static void Apply(Worker worker, WorkerBonus bonus, bool add)
    {
        var bonusManager = worker.GetBonusTracker();
        if (add)
        {
            bonusManager.AddOrUpdate(new(bonus.Id, bonus.Bonuses));
        }
        else
        {
            bonusManager.Remove(bonus.Id);
        }
    }

    public IEnumerable<EntityEffectDescription> DescribeAll(ILoc t, IDayNightCycle dayNightCycle)
    {
        if (activeBonuses.Count == 0) { yield break; }

        foreach (var b in activeBonuses.Values)
        {
            yield return new(b.Title, b.Desc(t),
                b.EndDay is null ? null : ((b.EndDay - dayNightCycle.PartialDayNumber) * 24f));
        }
    }
}

public record WorkerBonus(
    string Id,
    string Title,
    Func<ILoc, string> Desc,
    float? EndDay,
    ImmutableArray<BonusSpec> Bonuses
);
