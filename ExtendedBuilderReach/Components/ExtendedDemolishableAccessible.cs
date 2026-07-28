namespace ExtendedBuilderReach.Components;

public class ExtendedDemolishableAccessible(
    INavMeshListenerEntityRegistry navRegistry,
    MapSize mapSize
) : BaseComponent, IAccessibleNeeder, INavMeshListener, IAwakableComponent, IInitializableEntity, IDeletableEntity
{
    public const string AccessibleName = "ExtendedDemolishable";

    BoundingBox bounds;
    bool registered;

#nullable disable
    Accessible accessible;
    BlockObject blockObject;
    BlockObjectAccessGenerator blockObjectAccessGenerator;
#nullable enable

    public Accessible Accessible => accessible;

    public string AccessibleComponentName => AccessibleName;

    public void SetAccessible(Accessible accessible)
    {
        this.accessible = accessible;
    }

    public void Awake()
    {
        blockObject = GetComponent<BlockObject>();
        blockObjectAccessGenerator = GetComponent<BlockObjectAccessGenerator>();
    }

    public void InitializeEntity()
    {
        if (!MSettings.ExtendDemolishValue) { return; }

        UpdateAccesses();
        RegisterNavListener();
    }

    public void DeleteEntity()
    {
        UnregisterNavListener();
    }

    public void OnNavMeshUpdated(NavMeshUpdate navMeshUpdate)
    {
        if (!MSettings.ExtendDemolishValue) { return; }

        if (bounds.Intersects(navMeshUpdate.Bounds))
        {
            UpdateAccesses();
        }
    }

    public void UpdateAccesses()
    {
        if (!accessible) { return; }

        var z = blockObject.CoordinatesAtBaseZ.z;
        var minZ = ModUtils.GetMinZ(z);
        var maxZ = ModUtils.GetMaxZ(z, mapSize.TotalSize.z);

        bounds = blockObjectAccessGenerator.GenerateAccessBounds(minZ, maxZ);
        accessible.SetAccesses(blockObjectAccessGenerator.GenerateAccesses(minZ, maxZ));
        // Keep disabled so vanilla GetEnabledComponent<Accessible>() still resolves uniquely.
        accessible.DisableComponent();
    }

    void RegisterNavListener()
    {
        if (registered) { return; }

        navRegistry.RegisterNavMeshListener(this);
        registered = true;
    }

    void UnregisterNavListener()
    {
        if (!registered) { return; }

        navRegistry.UnregisterNavMeshListener(this);
        registered = false;
    }
}
