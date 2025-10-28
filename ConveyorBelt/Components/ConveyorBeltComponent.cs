namespace ConveyorBelt.Components;

public class ConveyorBeltComponent : BaseComponent, ISelectionListener, IFinishedStateListener
{

#nullable disable
    ConveyorBeltService service;
    ConveyorBeltSpec spec;
    BlockableBuilding blockableBuilding;
#nullable enable

    public bool IsLift => spec.IsLift;
    public ConveyorBeltConnection Connection { get; private set; } = new();
    public ConveyorBeltCluster Cluster { get; private set; }
    public bool Finished { get; private set; }
    public bool Active => Finished && blockableBuilding.IsUnblocked;

    [Inject]
    public void Inject(ConveyorBeltService service)
    {
        this.service = service;
    }

    public void Awake()
    {
        spec = GetComponentFast<ConveyorBeltSpec>();
        blockableBuilding = GetComponentFast<BlockableBuilding>();

        blockableBuilding.BuildingBlocked += OnBlocked;
        blockableBuilding.BuildingUnblocked += OnUnblocked;
    }

    internal void SetCluster(ConveyorBeltCluster cluster)
    {
        Cluster = cluster;
    }

    void OnUnblocked(object sender, EventArgs e)
    {
        TryToRegister();
    }

    void OnBlocked(object sender, EventArgs e)
    {
        service.Unregister(this);
    }

    public void OnEnterFinishedState()
    {
        SetCoordinates();
        Finished = true;

        TryToRegister();
    }

    public void OnExitFinishedState()
    {
        Finished = false;
        service.Unregister(this);
    }

    void TryToRegister()
    {
        if (Active)
        {
            service.Register(this);
        }
    }

    void SetCoordinates()
    {
        var blockObject = GetComponentFast<BlockObject>();
        var placement = blockObject.Placement;
        var coord = placement.Coordinates;

        Vector3Int prev, next;

        if (IsLift)
        {
            prev = coord + ConveyorBeltService.Below;
            next = coord + ConveyorBeltService.Above;
        }
        else
        {
            var orientationIndex = (int)placement.Orientation;
            prev = coord + ConveyorBeltService.BeltOrientationPrevious[orientationIndex];
            next = coord + ConveyorBeltService.BeltOrientationNext[orientationIndex];
        }

        Connection = new()
        {
            Coordinates = coord,
            PreviousCoords = prev,
            NextCoords = next,
        };
    }

    public void OnSelect()
    {
        if (!Active) { return; }
        service.HighlighCluster(this);
    }

    public void OnUnselect()
    {
        if (!Active) { return; }
        service.UnhighlightCluster();
    }
}
