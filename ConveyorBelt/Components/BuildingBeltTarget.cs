
namespace ConveyorBelt.Components;

[AddTemplateModule2(typeof(BuildingSpec))]
public class BuildingBeltTarget(BeltGoodService goods) : BaseComponent, IBeltConnectionProvider, IAwakableComponent
{
    BlockObject block = null!;

    public int Priority => BeltConnectionPriority.Building;

    public void Awake() => block = GetComponent<BlockObject>();

    public bool TryProvide(BeltApproach approach, out IBeltConnection connection)
    {
        connection = MissingConnection.Instance;
        if (this.GetComponentOrNull<BeltCarrier>() || this.GetComponentOrNull<BeltMerger>() || this.GetComponentOrNull<BeltSplitter>())
        {
            return false;
        }

        if (this.GetComponentOrNull<Inventories>() is not { } inventories)
        {
            return false;
        }

        connection = new InventoryConnection(block, inventories, goods);
        return true;
    }
}
