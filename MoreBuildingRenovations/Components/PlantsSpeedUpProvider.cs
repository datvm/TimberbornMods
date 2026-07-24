namespace MoreBuildingRenovations.Components;

[AddTemplateModule2(typeof(PlantsSpeedUpSpec))]
public class PlantsSpeedUpProvider(
    PlantSpeedUpService service
) : BaseComponent, IAwakableComponent, IFinishedStateListener, IEntityEffectDescriber
{
    PlantsSpeedUpSpec spec = null!;
    BuildingTerrainRange terrainRange = null!;
    Workplace workplace = null!;

    readonly HashSet<SpeedablePlant> affectingPlants = [];

    public PlantsSpeedUpType AffectingType => spec.AffectingType;
    public IReadOnlyCollection<SpeedablePlant> AffectingPlants => affectingPlants;

    public bool IsActivated { get; private set; }
    float bonus;

    public bool HasIdleWorker
    {
        get
        {
            foreach (var worker in workplace.AssignedWorkers)
            {
                if (worker._behaviorManager.IsRunningBehavior<WaitInsideIdlyWorkplaceBehavior>())
                {
                    return true;
                }
            }

            return false;
        }
    }

    public void Awake()
    {
        spec = GetComponent<PlantsSpeedUpSpec>();
        terrainRange = GetComponent<BuildingTerrainRange>();
        workplace = GetComponent<Workplace>();
    }

    public void Activate(float speedBonus)
    {
        bonus = speedBonus;
        IsActivated = true;
        terrainRange.RangeChanged += OnRangeChanged;
        UpdateAffectingPlants();
        service.Register(this, speedBonus);
    }

    void OnRangeChanged(object sender, RangeChangedEventArgs e) => UpdateAffectingPlants();

    public void Deactivate()
    {
        IsActivated = false;
        terrainRange?.RangeChanged -= OnRangeChanged;
        service.Unregister(this);
        affectingPlants.Clear();
    }

    public void UpdateAffectingPlants()
    {
        affectingPlants.Clear();
        if (!IsActivated) { return; }

        foreach (var p in service.GetPlantsInRange(terrainRange.GetRange()._set))
        {
            if (Matches(p))
            {
                affectingPlants.Add(p);
            }
        }
    }

    public void OnSpeedablePlantRemoved(SpeedablePlant p) => affectingPlants.Remove(p);

    public void OnSpeedablePlantAdded(SpeedablePlant plant)
    {
        if (IsActivated && Matches(plant) && terrainRange.GetRange().Contains(plant.Coordinates))
        {
            affectingPlants.Add(plant);
        }
    }

    bool Matches(SpeedablePlant speedablePlant) => AffectingType switch
    {
        PlantsSpeedUpType.Crops => speedablePlant.IsCrop,
        PlantsSpeedUpType.Trees => speedablePlant.IsTree,
        PlantsSpeedUpType.TreeProducts => speedablePlant.HasTreeProduct,
        PlantsSpeedUpType.BushProducts => speedablePlant.HasBushProduct,
        _ => throw new NotSupportedException($"Unsupported PlantsSpeedUpType: {AffectingType}"),
    };

    public void OnEnterFinishedState() { }

    public void OnExitFinishedState()
    {
        Deactivate();
    }

    public EntityEffectDescription? Describe(ILoc t, IDayNightCycle dayNightCycle)
    {
        if (!IsActivated) { return null; }

        var key = HasIdleWorker ? "LV.MBR.PlantsSpeedBuffWithWorker" : "LV.MBR.PlantsSpeedBuffNoWorker";
        var actionKey = "LV.MBR.PlantsSpeedBuff." + AffectingType;

        return new(
            t.T("LV.MBR.PlantsSpeedIdle"),
            t.T(key, t.T(actionKey, bonus, AffectingPlants.Count))
        );
    }
}
