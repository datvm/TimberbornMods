namespace MoreBuildingRenovations.Services;

[BindSingleton]
public class PlantSpeedUpService(
    IDayNightCycle dayNightCycle,
    IBlockService blockService
) : ITickableSingleton
{
    static readonly ImmutableArray<PlantsSpeedUpType> AllTypes = TimberUiUtils.GetSortedEnumValues<PlantsSpeedUpType>();

    readonly DeferredHashSet<PlantsSpeedUpProvider> comps = [];
    readonly ImmutableArray<HashSet<SpeedablePlant>> currAffecting = [.. AllTypes.Select(_ => new HashSet<SpeedablePlant>())];

    float speedBonus;

    public HashSet<SpeedablePlant> GetPlantsInRange(IEnumerable<Vector3Int> coords)
    {
        HashSet<SpeedablePlant> plants = [];
        foreach (var coord in coords)
        {
            var p = blockService.GetFirstObjectWithComponentAt<SpeedablePlant>(coord);
            if (p)
            {
                plants.Add(p);
            }
        }

        return plants;
    }

    public void Register(PlantsSpeedUpProvider comp, float speedBonus)
    {
        this.speedBonus = speedBonus;
        comps.Add(comp);
    }

    public void Unregister(PlantsSpeedUpProvider comp) => comps.Remove(comp);

    public void Tick()
    {
        if (comps.Count == 0)
        {
            return;
        }

        if (!GatherPlants()) { return; }

        SpeedUpPlants();
        CleanUp();
    }

    public void OnPlantAdded(SpeedablePlant plant)
    {
        foreach (var c in comps)
        {
            c.OnSpeedablePlantAdded(plant);
        }
    }

    public void OnPlantDeleted(SpeedablePlant plant)
    {
        foreach (var c in comps)
        {
            c.OnSpeedablePlantRemoved(plant);
        }
    }

    bool GatherPlants()
    {
        var hasPlant = false;

        foreach (var c in comps)
        {
            if (!c || !c.IsActivated) // Should not happen, but just in case
            {
                Unregister(c);
                continue;
            }

            if (!c.HasIdleWorker)
            {
                continue;
            }

            var typeIndex = (int)c.AffectingType;
            var list = currAffecting[typeIndex];

            list.UnionWith(c.AffectingPlants);

            if (!hasPlant && list.Count > 0)
            {
                hasPlant = true;
            }
        }

        return hasPlant;
    }

    void SpeedUpPlants()
    {
        var deltaDays = dayNightCycle.FixedDeltaTimeInHours / 24f * speedBonus;

        foreach (var t in AllTypes)
        {
            var list = currAffecting[(int)t];
            if (list.Count == 0)
            {
                continue;
            }

            foreach (var plant in list)
            {
                if (!plant)
                {
                    continue;
                }

                ApplySpeedBonus(plant, t, deltaDays);
            }
        }
    }

    void ApplySpeedBonus(SpeedablePlant plant, PlantsSpeedUpType type, float bonusDelta)
    {
        switch (type)
        {
            case PlantsSpeedUpType.Crops:
            case PlantsSpeedUpType.Trees:
                plant.BoostGrowth(bonusDelta);
                break;
            case PlantsSpeedUpType.BushProducts:
            case PlantsSpeedUpType.TreeProducts:
                plant.BoostProductGrowth(bonusDelta);
                break;
        }
    }

    void CleanUp()
    {
        for (var i = 0; i < currAffecting.Length; i++)
        {
            currAffecting[i].Clear();
        }
    }
}
