namespace MoreBuildingRenovations.Components;

[AddTemplateModule2(typeof(Workplace))]
public class WorkplaceBonuses : BaseComponent, IAwakableComponent
{

    Workplace workplace = null!;
    BonusDescriptionComponent bonusDescription = null!;
    readonly Dictionary<string, WorkerBonus> activeBonuses = [];

    public void Awake()
    {
        workplace = GetComponent<Workplace>();
        bonusDescription = GetComponent<BonusDescriptionComponent>();
        workplace.WorkerAssigned += OnWorkerAssigned;
        workplace.WorkerUnassigned += OnWorkerUnassigned;
    }

    public void AddBonus(WorkerBonus bonus)
    {
        RemoveBonus(bonus.Id);

        activeBonuses[bonus.Id] = bonus;
        ApplyToAssignedWorkers(bonus, add: true);
        bonusDescription.AddBonus(bonus.BonusDescription);
    }

    public void RemoveBonus(string id)
    {
        if (!activeBonuses.Remove(id, out var bonus)) { return; }

        ApplyToAssignedWorkers(bonus, add: false);
        bonusDescription.RemoveBonus(id);
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
}

public record WorkerBonus(
    ImmutableArray<BonusSpec> Bonuses,
    BonusDescription BonusDescription
)
{
    public string Id => BonusDescription.Id;
}
