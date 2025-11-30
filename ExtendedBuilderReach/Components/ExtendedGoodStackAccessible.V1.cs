namespace ExtendedBuilderReach.Components;

public class ExtendedDemolishableAccessible(INavMeshListenerEntityRegistry navRegistry) : BaseComponent, INavMeshListener, IAwakableComponent, IStartableComponent, IFinishedStateListener
{
    BoundingBox bounds;

#nullable disable
    internal Accessible accessible;
    BlockObject blockObject;
    BlockObjectAccessGenerator blockObjectAccessGenerator;
#nullable enable

    public Accessible Accessible => accessible;

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
        var (minZ, maxZ) = GetMinMaxZ();
        bounds = blockObjectAccessGenerator.GenerateAccessBounds(minZ, maxZ);

        UpdateAccesses();
    }

    public void UpdateAccesses()
    {
        var (minZ, maxZ) = GetMinMaxZ();
        accessible.SetAccesses(blockObjectAccessGenerator.GenerateAccesses(minZ, maxZ));
    }

    KeyValuePair<int, int> GetMinMaxZ()
    {
        var z = blockObject.CoordinatesAtBaseZ.z;

        var minZ = ModUtils.GetMinZ(z);
        var maxZ = ModUtils.GetMaxZ(z, blockObject._blockService._mapSize.TotalSize.z);

        return new(minZ, maxZ);
    }

    public void OnEnterFinishedState()
    {
        navRegistry.RegisterNavMeshListener(this);
    }

    public void OnExitFinishedState()
    {
        navRegistry.UnregisterNavMeshListener(this);
    }
}
