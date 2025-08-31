namespace BuildingHP.Components.Renovations;

public interface IWorkplaceProductivityComponent
{

    HashSet<BonusManager> WorkerBonus { get; }
    float ProductivityMultiplier { get; set; }

    EventHandler<WorkerChangedEventArgs>? WorkerAssigned { get; set; }
    EventHandler<WorkerChangedEventArgs>? WorkerUnassigned { get; set; }

}

public static class WorkplaceProductivityComponentExtensions
{
    const string WorkBonusId = "WorkingSpeed";

    public static void SetWorkplaceProductivity<T>(this T c, float additional)
        where T : BaseComponent, IWorkplaceProductivityComponent
    {
        ResetWorkplaceProductivity(c);

        c.ProductivityMultiplier = additional;
        var workplace = c.GetComponentFast<Workplace>();

        c.WorkerAssigned = (_, e) =>
        {
            var bonus = e.Worker.GetBonusManager();
            AddWorkBonus(bonus, additional);
            c.WorkerBonus.Add(bonus);
        };
        workplace.WorkerAssigned += c.WorkerAssigned;

        c.WorkerUnassigned = (_, e) =>
        {
            var bonus = e.Worker.GetBonusManager();
            RemoveWorkBonus(bonus, additional);
            c.WorkerBonus.Remove(bonus);
        };
        workplace.WorkerUnassigned += c.WorkerUnassigned;

        foreach (var w in workplace.AssignedWorkers)
        {
            var bonus = w.GetBonusManager();
            AddWorkBonus(bonus, additional);
            c.WorkerBonus.Add(bonus);
        }
    }

    public static void ResetWorkplaceProductivity<T>(this T c)
        where T : BaseComponent, IWorkplaceProductivityComponent
    {
        var workplace = c.GetComponentFast<Workplace>();
        if (c.WorkerAssigned is not null)
        {
            workplace.WorkerAssigned -= c.WorkerAssigned;
            c.WorkerAssigned = null;
        }
        if (c.WorkerUnassigned is not null)
        {
            workplace.WorkerUnassigned -= c.WorkerUnassigned;
            c.WorkerUnassigned = null;
        }

        var mul = c.ProductivityMultiplier;
        foreach (var w in c.WorkerBonus)
        {
            RemoveWorkBonus(w, mul);
        }
        c.ProductivityMultiplier = 0f;

        c.WorkerBonus.Clear();
    }

    static BonusManager GetBonusManager<T>(this T comp) where T : BaseComponent => comp.GetComponentFast<BonusManager>();

    static void AddWorkBonus(BonusManager bonusManager, float mul)
    {
        bonusManager.AddBonus(WorkBonusId, mul);
    }

    static void RemoveWorkBonus(BonusManager bonusManager, float mul)
    {
        bonusManager.RemoveBonus(WorkBonusId, mul);
    }

}