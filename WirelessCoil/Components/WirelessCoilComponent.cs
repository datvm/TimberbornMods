
namespace WirelessCoil.Components;

public class WirelessCoilComponent : BaseComponent, IFinishedStateListener
{

#nullable disable
    WirelessCoilService service;
    BlockObject blockObject;

    public WirelessCoilSpec Spec { get; private set; }
    MechanicalNode mechanicalNode;
#nullable enable

    public IReadOnlyList<WirelessCoilComponent> ConnectedCoils { get; set; } = [];
    public Vector3Int Coordinates => blockObject.Coordinates;
    public MechanicalGraph? MechanicalGraph => mechanicalNode.Graph;

    public int Range => Spec.Range;

    [Inject]
    public void Inject(WirelessCoilService service)
    {
        this.service = service;
    }

    public void Awake()
    {
        Spec = GetComponentFast<WirelessCoilSpec>();
        blockObject = GetComponentFast<BlockObject>();
        mechanicalNode = GetComponentFast<MechanicalNode>();
    }

    public void OnEnterFinishedState()
    {
        service.Register(this);
    }

    public void OnExitFinishedState()
    {
        service.Unregister(this);
    }

}
