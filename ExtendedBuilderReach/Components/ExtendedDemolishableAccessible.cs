namespace ExtendedBuilderReach.Components;

public class ExtendedDemolishableAccessible : BaseComponent, INavMeshListener, IInitializableEntity, IDeletableEntity
{
    public const string AccessibleComponentName = nameof(Demolishable);

#nullable disable
    public Accessible Accessible { get; private set; } = null!;

    BlockObjectAccessGenerator blockObjectAccessGenerator;
    BlockObject blockObject;

    BaseInstantiator baseInstantiator;
    NavMeshListenerEntityRegistry navMeshListenerEntityRegistry;
    MapSize mapSize;
#nullable enable

    BoundingBox bounds;

    [Inject]
    public void Inject(BaseInstantiator baseInstantiator, NavMeshListenerEntityRegistry navMeshListenerEntityRegistry, MapSize mapSize)
    {
        this.baseInstantiator = baseInstantiator;
        this.navMeshListenerEntityRegistry = navMeshListenerEntityRegistry;
        this.mapSize = mapSize;
    }

    public void Awake()
    {
        blockObjectAccessGenerator = GetComponentFast<BlockObjectAccessGenerator>();
        blockObject = GetComponentFast<BlockObject>();

        Accessible = baseInstantiator.AddComponent<Accessible>(GameObjectFast);
        Accessible.Initialize(AccessibleComponentName);
    }

    public void OnNavMeshUpdated(NavMeshUpdate navMeshUpdate)
    {
        if (bounds.Intersects(navMeshUpdate.Bounds))
        {
            UpdateAccess();
        }
    }

    void UpdateAccess()
    {
        var z = blockObject.CoordinatesAtBaseZ.z;

        var minZ = ModUtils.GetMinZ(z);
        var maxZ = ModUtils.GetMaxZ(z, mapSize.TotalSize.z);

        bounds = blockObjectAccessGenerator.GenerateAccessBounds(minZ, maxZ);
        Accessible.SetAccesses(blockObjectAccessGenerator.GenerateAccesses(minZ, maxZ));
    }

    public void DeleteEntity()
    {
        navMeshListenerEntityRegistry.UnregisterNavMeshListener(this);
    }

    public void InitializeEntity()
    {
        navMeshListenerEntityRegistry.RegisterNavMeshListener(this);
        UpdateAccess();
    }
}
