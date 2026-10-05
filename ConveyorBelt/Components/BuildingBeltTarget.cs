namespace ConveyorBelt.Components;

[AddTemplateModule2(typeof(BuildingSpec))]
public class BuildingBeltTarget(BeltGoodService goods) : BaseComponent, IBeltConnectionProvider, IAwakableComponent
{
    BlockObject block = null!;

    public int Priority => BeltConnectionPriority.Building;

    public void Awake() => block = GetComponent<BlockObject>();

    public bool TryProvide(BeltApproach approach, SimBelt? upstream, out ISimLink link)
    {
        link = SimLinks.None;
        if (upstream is { Plain: false })
        {
            return false;
        }

        if (BeltBlocks.HostsBelt(this))
        {
            return false;
        }

        if (this.GetComponentOrNull<Inventories>() is not { } inventories)
        {
            return false;
        }

        link = new InventoryConnection(block, inventories, goods);
        return true;
    }
}
