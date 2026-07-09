namespace ConveyorBelt.Components;

[AddTemplateModule2(typeof(ConveyorBeltJunctionSpec))]
public class ConveyorBeltJunction : BaseComponent, IInitializableEntity
{
    int inputIndex;
    public ImmutableArray<Vector3Int> InputCoordinates { get; private set; }
    public Vector3Int OutputCoordinates { get; set; }

    BlockObject bo = null!;
    MechanicalBuilding mechanicalBuilding = null!;
    public Vector3Int Coordinates => bo.Coordinates;
    public bool CanUse => bo.IsFinished && mechanicalBuilding.CanUse;

    public void InitializeEntity()
    {
        bo = GetComponent<BlockObject>();
        mechanicalBuilding = GetComponent<MechanicalBuilding>();
        var spec = GetComponent<ConveyorBeltJunctionSpec>();

        InputCoordinates = [.. spec.InputCoordinates.Select(bo.TransformCoordinates)];
        OutputCoordinates = bo.TransformCoordinates(spec.OutputCoordinates);
    }

    public IEnumerable<Vector3Int> GetInputCoordinates()
    {
        var len = InputCoordinates.Length;
        for (int i = 0; i < len; i++)
        {
            inputIndex = (inputIndex + 1) % len;
            yield return InputCoordinates[inputIndex];
        }
    }

}
