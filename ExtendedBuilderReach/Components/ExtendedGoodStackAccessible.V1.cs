namespace ExtendedBuilderReach.Components;

public class ExtendedDemolishableAccessible : BaseComponent, INavMeshListener, IAwakableComponent, IStartableComponent
{
    BoundingBox bounds;

#nullable disable
    internal Accessible accessible;
    BlockObject blockObject;
    BlockObjectAccessGenerator blockObjectAccessGenerator;
#nullable enable

    public void Awake()
    {
        accessible = GetComponent<Accessible>();
        blockObject = GetComponent<BlockObject>();
        blockObjectAccessGenerator = GetComponent<BlockObjectAccessGenerator>();
    }

    public void OnNavMeshUpdated(NavMeshUpdate navMeshUpdate)
    {
        if (bounds.Intersects(navMeshUpdate.Bounds))
        {
            UpdateAccesses();
        }
    }

    public void Start()
    {
        UpdateAccesses();
    }

    public void UpdateAccesses()
    {
        var z = blockObject.CoordinatesAtBaseZ.z;

        var minZ = ModUtils.GetMinZ(z);
        var maxZ = ModUtils.GetMaxZ(z, blockObject._blockService._mapSize.TotalSize.z);

        bounds = blockObjectAccessGenerator.GenerateAccessBounds(minZ, maxZ);
        accessible.SetAccesses(blockObjectAccessGenerator.GenerateAccesses(minZ, maxZ));
    }

}
