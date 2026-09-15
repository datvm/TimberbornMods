namespace ConveyorBelt.Components;

[AddTemplateModule2(typeof(ConveyorBeltJunctionSpec))]
public class ConveyorBeltJunction : BaseComponent, IAwakableComponent, IInitializableEntity
{
    int inputIndex;
    int outputIndex;
    public ImmutableArray<Vector3Int> InputCoordinates { get; private set; }
    public ImmutableArray<Vector3Int> OutputCoordinates { get; private set; }

#nullable disable
    BlockObject bo;
    MechanicalNode mechanicalNode;
#nullable enable

    public Vector3Int Coordinates => bo.Coordinates;
    public bool CanUse => bo.IsFinished && (!mechanicalNode.IsConsumer || mechanicalNode.ActiveAndPowered);

    public void Awake()
    {
        bo = GetComponent<BlockObject>();
        mechanicalNode = GetComponent<MechanicalNode>();
    }

    public void InitializeEntity()
    {
        var spec = GetComponent<ConveyorBeltJunctionSpec>();
        InputCoordinates = [.. spec.InputCoordinates.Select(bo.TransformCoordinates)];
        OutputCoordinates = [.. spec.OutputCoordinates.Select(bo.TransformCoordinates)];
    }

    public IEnumerable<Vector3Int> GetInputCoordinates()
    {
        var len = InputCoordinates.Length;
        for (var i = 0; i < len; i++)
        {
            inputIndex = (inputIndex + 1) % len;
            yield return InputCoordinates[inputIndex];
        }
    }

    public IEnumerable<Vector3Int> GetOutputCoordinates()
    {
        var len = OutputCoordinates.Length;
        for (var i = 0; i < len; i++)
        {
            outputIndex = (outputIndex + 1) % len;
            yield return OutputCoordinates[outputIndex];
        }
    }
}
