namespace ConveyorBelt.Components;

[AddTemplateModule2(typeof(FloodableBeltSpec))]
public class FloodableBelt(ILoc t) : BaseComponent, IWaterObjectSpecification, IAwakableComponent, IInitializableEntity, IFinishedStateListener, IEntityDescriber
{
    public Vector3Int WaterCoordinates => Vector3Int.zero;

    WaterObject water = null!;
    SimBelt lane = null!;
    StatusToggle status = null!;
    bool flooded;

    public void Awake()
    {
        water = GetComponent<WaterObject>();
        lane = FindLane();
        status = StatusToggle.CreateNormalStatusWithAlertAndFloatingIcon(
            "FloodedBuilding",
            t.T("LV.CBlt.BeltFloodable"),
            t.T("LV.CBlt.BeltFloodedShort"));
    }

    public void InitializeEntity() => GetComponent<StatusSubject>().RegisterStatus(status);

    public void OnEnterFinishedState()
    {
        water.WaterAboveBaseChanged += OnWater;
        SetFlooded(water.WaterAboveBase > 0);
    }

    public void OnExitFinishedState()
    {
        water.WaterAboveBaseChanged -= OnWater;
        SetFlooded(false);
    }

    public IEnumerable<EntityDescription> DescribeEntity()
        => [EntityDescription.CreateTextSection($"{SpecialStrings.RowStarter} {t.T("LV.CBlt.BeltFloodable")}", 200)];

    void OnWater(object sender, EventArgs e) => SetFlooded(water.WaterAboveBase > 0);

    void SetFlooded(bool value)
    {
        if (flooded == value)
        {
            return;
        }

        flooded = value;
        lane.Flooded = value;
        if (!value)
        {
            status.Deactivate();
            return;
        }

        lane.Clear();
        status.Activate();
    }

    SimBelt FindLane()
    {
        if (GetComponent<BeltCarrier>() is { } carrier)
        {
            return carrier.Sim;
        }

        if (GetComponent<BeltMerger>() is { } merger)
        {
            return merger.Sim.Belt;
        }

        if (GetComponent<BeltLift>() is { } lift)
        {
            return lift.Sim.Belt;
        }

        return GetComponent<BeltSplitter>().Sim.Belt;
    }
}
