namespace BeaverChronicles.Services;

[BindSingleton]
public class WorkplaceHelper(
    WorkplaceTracker workplaces,
    IDayNightCycle dayNightCycle
) : ITickableSingleton, ILoadableSingleton
{
    public delegate bool WorkplaceFilter(Workplace workplace);

    readonly Dictionary<string, WorkplaceLimitedTimeStatus> activeBonuses = [];

    public void Load()
    {
        workplaces.OnEntityRegistered += OnEntityAdded;
    }

    void OnEntityAdded(WorkplaceTrackerComponent obj)
    {
        var comp = GetWorkplaceBonusComp(obj);
        foreach (var bonus in activeBonuses.Values)
        {
            if (bonus.WorkplaceFilter(obj.Workplace))
            {
                comp.AddOrUpdateBonus(bonus);
            }
        }
    }

    public bool HasMinimumWorkers(int expected, WorkplaceFilter workplaceFilter)
    {
        if (expected <= 0) { return true; }

        var counter = 0;

        foreach (var workplace in GetWorkplaces(workplaceFilter))
        {
            counter += workplace.AssignedWorkers.Count;

            if (counter >= expected)
            {
                return true;
            }
        }

        return false;
    }

    public void AddOrUpdateWorkplaceBonus(WorkplaceLimitedTimeStatus bonus, float? days)
    {
        if (days.HasValue)
        {
            bonus = bonus with { UntilDay = dayNightCycle.PartialDayNumber + days.Value };
        }

        if (activeBonuses.ContainsKey(bonus.Id))
        {
            RemoveWorkplaceBonus(bonus.Id);
        }

        activeBonuses[bonus.Id] = bonus;
        foreach (var workplace in GetWorkplaces(bonus.WorkplaceFilter))
        {
            GetWorkplaceBonusComp(workplace).AddOrUpdateBonus(bonus);
        }
    }

    public void RemoveWorkplaceBonus(string bonusId)
    {
        if (!activeBonuses.TryGetValue(bonusId, out var bonus)) { return; }

        foreach (var workplace in GetWorkplaces(bonus.WorkplaceFilter))
        {
            GetWorkplaceBonusComp(workplace).RemoveBonus(bonusId);
        }

        activeBonuses.Remove(bonusId);
    }

    public static bool IsFarmhouse(Workplace workplace)
    {
        var spec = workplace.GetComponent<PlanterBuildingSpec>();
        return spec is not null && spec.PlantableResourceGroup == "Farmhouse";
    }

    public static bool IsLumberjack(Workplace workplace)
    {
        var spec = workplace.GetComponent<YieldRemovingBuildingSpec>();
        return spec is not null && spec.ResourceGroup == "Cuttable";
    }

    public static bool AllWorkplaces(Workplace _) => true;

    public void Tick()
    {
        if (activeBonuses.Count == 0) { return; }

        var day = dayNightCycle.PartialDayNumber;
        var expiredBonuses = activeBonuses.Values.Where(b => b.UntilDay <= day).ToArray();
        foreach (var b in expiredBonuses)
        {
            RemoveWorkplaceBonus(b.Id);
        }
    }

    public IEnumerable<Workplace> GetWorkplaces(WorkplaceFilter workplaceFilter)
    {
        foreach (var e in workplaces.Entities)
        {
            var workplace = e.Workplace;
            if (workplaceFilter(workplace))
            {
                yield return workplace;
            }
        }
    }

    static WorkplaceWorkerBonusComponent GetWorkplaceBonusComp(BaseComponent comp) => comp.GetComponent<WorkplaceWorkerBonusComponent>();

}
