namespace ConveyorBelt.Components;

public abstract class BeltJunction : BaseComponent, IBeltConnectionProvider, IAwakableComponent, IInitializableEntity, IFinishedStateListener
{
    readonly List<BeltPort> localInputs = [];
    readonly List<BeltPort> localOutputs = [];

    BlockObject block = null!;
    BlockableObject blockable = null!;
    MechanicalNode mechanical = null!;

    public Vector3Int Coordinates => block.Coordinates;
    public bool Running => blockable.IsUnblocked && (!mechanical.IsConsumer || mechanical.PowerEfficiency >= 1f);
    public Vector3Int[] InputCells { get; private set; } = [];
    public Vector3Int[] OutputCells { get; private set; } = [];
    public int Priority => BeltConnectionPriority.Conveyor;

    protected abstract void FillPorts(List<BeltPort> inputs, List<BeltPort> outputs);

    public abstract bool TryProvide(BeltApproach approach, SimBelt? upstream, out ISimLink link);

    protected virtual void Entered()
    {
    }

    protected virtual void Exited()
    {
    }

    public void Awake()
    {
        block = GetComponent<BlockObject>();
        blockable = GetComponent<BlockableObject>();
        mechanical = GetComponent<MechanicalNode>();
    }

    public void InitializeEntity() => RebuildSides();

    public void OnEnterFinishedState()
    {
        RebuildSides();
        Entered();
    }

    public void OnExitFinishedState() => Exited();

    public bool AcceptsInput(Vector3Int from)
    {
        if (!block.IsFinished)
        {
            return false;
        }

        for (var i = 0; i < InputCells.Length; i++)
        {
            if (InputCells[i] == from)
            {
                return true;
            }
        }

        return false;
    }

    protected void RebuildSides()
    {
        FillPorts(localInputs, localOutputs);
        InputCells = Cells(localInputs);
        OutputCells = Cells(localOutputs);
    }

    Vector3Int[] Cells(List<BeltPort> ports)
    {
        var cells = new Vector3Int[ports.Count];
        for (var i = 0; i < ports.Count; i++)
        {
            cells[i] = block.Coordinates + block.TransformDirection(ports[i].Direction.Game()).ToOffset();
        }

        return cells;
    }
}
