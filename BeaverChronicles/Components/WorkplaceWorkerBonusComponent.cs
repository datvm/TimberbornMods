namespace BeaverChronicles.Components;

[AddTemplateModule2(typeof(Workplace))]
public class WorkplaceWorkerBonusComponent : BaseComponent, IAwakableComponent, IDeletableEntity
{
    readonly record struct StatusBonusPair(WorkplaceLimitedTimeStatus Status, BonusTrackerItem Bonus);

    readonly Dictionary<string, StatusBonusPair> activeBonuses = [];
    readonly HashSet<BonusTrackerComponent> workers = [];


#nullable disable
    StatusDescriptionComponent statusDescriptionComponent;
    Workplace workplace;
#nullable enable

    public void Awake()
    {
        statusDescriptionComponent = GetComponent<StatusDescriptionComponent>();
        workplace = GetComponent<Workplace>();

        workplace.WorkerAssigned += OnWorkerAdded;
        workplace.WorkerUnassigned += OnWorkerRemoved;

        foreach (var worker in workplace.AssignedWorkers)
        {
            OnWorkerAdded(this, new(worker));
        }
    }

    void OnWorkerRemoved(object sender, WorkerChangedEventArgs e)
    {
        var comp = e.Worker.GetBonusTracker();
        workers.Remove(comp);
        ToggleBonus(comp, false);
    }

    void OnWorkerAdded(object sender, WorkerChangedEventArgs e)
    {
        var comp = e.Worker.GetBonusTracker();
        workers.Add(comp);
        ToggleBonus(comp, true);
    }

    void ToggleBonus(BonusTrackerComponent comp, bool add)
    {
        var tracker = comp.GetBonusTracker();
        foreach (var (_, b) in activeBonuses.Values)
        {
            if (add)
            {
                tracker.AddOrUpdate(b);
            }
            else
            {
                tracker.Remove(b.Id);
            }
        }
    }

    public void AddOrUpdateBonus(WorkplaceLimitedTimeStatus status)
    {
        if (activeBonuses.ContainsKey(status.Id))
        {
            RemoveBonus(status.Id);
        }

        StatusBonusPair pair = new(status, new(status.Id, [.. status.Bonuses.Select(b => b.ToBonusSpec())]));
        activeBonuses[status.Id] = pair;
        statusDescriptionComponent.AddStatus(status);

        foreach (var w in workers)
        {
            w.AddOrUpdate(pair.Bonus);
        }
    }

    public void RemoveBonus(string id)
    {
        statusDescriptionComponent.RemoveStatus(id);

        if (activeBonuses.Remove(id))
        {
            foreach (var w in workers)
            {
                w.Remove(id);
            }
        }
    }

    public void DeleteEntity()
    {
        foreach (var b in activeBonuses.Values.ToArray())
        {
            RemoveBonus(b.Status.Id);
        }
    }
}
