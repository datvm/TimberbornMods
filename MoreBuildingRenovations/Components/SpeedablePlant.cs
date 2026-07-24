namespace MoreBuildingRenovations.Components;

[AddTemplateModule2(typeof(Growable))]
public class SpeedablePlant(PlantSpeedUpService service) : BaseComponent, IAwakableComponent, IFinishedStateListener
{
    public Growable Growable { get; private set; } = null!;
    public bool IsCrop { get; private set; }
    public bool IsTree { get; private set; }
    public bool IsBush { get; private set; }
    public GatherableYieldGrower? Gatherable { get; private set; }

    public bool HasTreeProduct => IsTree && Gatherable;
    public bool HasBushProduct => IsBush && Gatherable;

    public Vector3Int Coordinates { get; private set; }

    float growthDays, productGrowthDays;

    public void Awake()
    {
        var g = Growable = GetComponent<Growable>();
        IsCrop = g.GetComponent<Crop>();
        IsTree = g.GetComponent<TreeComponent>();
        IsBush = g.HasComponent<BushSpec>();
        Gatherable = g.GetComponentOrNull<GatherableYieldGrower>();
    }

    public bool BoostGrowth(float deltaTime)
    {
        if (!Growable.GrowthInProgress || growthDays == 0) { return false; }
        Growable.IncreaseGrowthProgress(deltaTime / growthDays);
        return true;
    }

    public bool BoostProductGrowth(float deltaTime)
    {
        if (!Gatherable || !Gatherable!._timeTrigger.InProgress || productGrowthDays == 0) { return false; }

        Gatherable.FastForwardGrowth(deltaTime / productGrowthDays);
        return true;
    }

    public void OnEnterFinishedState()
    {
        Coordinates = GetComponent<BlockObject>().Coordinates;

        growthDays = Growable.GrowthTimeInDays;
        if (Gatherable)
        {
            productGrowthDays = Gatherable!._gatherable.YieldGrowthTimeInDays;
        }

        service.OnPlantAdded(this);
    }

    public void OnExitFinishedState() => service.OnPlantDeleted(this);
}