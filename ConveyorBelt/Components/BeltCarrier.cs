namespace ConveyorBelt.Components;

[AddTemplateModule2(typeof(BeltCarrierSpec))]
public class BeltCarrier(BeltCarrierService service)
    : BaseComponent, IBeltConnectionProvider, IAwakableComponent, IInitializableEntity, IFinishedStateListener, IEntityDescriber
{
    readonly List<BeltPort> localPorts = [];

    BlockObject block = null!;
    BlockableObject blockable = null!;
    MechanicalNode mechanical = null!;

    float? itemsPerHour;
    public float ItemsPerHour => itemsPerHour ??= service.ItemsPerHour(SpeedIndex);

    int? speedIndex;
    public int SpeedIndex => speedIndex ??= service.Index(GetComponent<BeltCarrierSpec>().Speed);

    public SimBelt Sim { get; } = new();
    public BeltShape Shape { get; private set; }
    public Vector3Int Coordinates => block.Coordinates;
    public BeltEnd InPort { get; private set; }
    public BeltEnd OutPort { get; private set; }
    public bool Running => blockable.IsUnblocked && (!mechanical.IsConsumer || mechanical.PowerEfficiency >= 1f);
    public int Priority => BeltConnectionPriority.Conveyor;

    public void Awake()
    {
        block = GetComponent<BlockObject>();
        blockable = GetComponent<BlockableObject>();
        mechanical = GetComponent<MechanicalNode>();
    }

    public void InitializeEntity()
    {
        var spec = GetComponent<BeltCarrierSpec>();
        Shape = spec.Shape;
        itemsPerHour = service.ItemsPerHour(SpeedIndex);
        Sim.ItemsPerHour = ItemsPerHour;
        Sim.CanCarry = service.IsCarryable;
        RebuildPorts();
    }

    public void OnEnterFinishedState()
    {
        RebuildPorts();
        service.Register(this);
    }

    public void OnExitFinishedState() => service.Unregister(this);

    public bool TryProvide(BeltApproach approach, SimBelt? upstream, out ISimLink link)
    {
        link = SimLinks.None;
        if (!block.IsFinished || approach.To != Coordinates || approach.From != InPort.Target)
        {
            return false;
        }

        link = SimLinks.ToBelt(Sim);
        return true;
    }

    public IEnumerable<EntityDescription> DescribeEntity() => [service.Describe(ItemsPerHour)];

    void RebuildPorts()
    {
        BeltLayout.FillCarrier((BeltForm)Shape, localPorts);
        foreach (var port in localPorts)
        {
            var end = new BeltEnd(block.TransformCoordinates(Vector3Int.zero), block.TransformDirection(port.Direction.Game()));
            if (port.Incoming)
            {
                InPort = end;
            }
            else
            {
                OutPort = end;
            }
        }
    }
}
