namespace ExtendedBuilderReach.Components;

public class ExtendedDemolishableAccessible(
    DemolitionNavigationService navigation,
    MapSize mapSize
) : BaseComponent, IAccessibleNeeder, IAwakableComponent, IInitializableEntity, IDeletableEntity
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
        if (!MSettings.ExtendDemolishValue)
        {
            return;
        }

        UpdateAccesses();
        navigation.Register(this, bounds);
        registered = true;
    }

    public void DeleteEntity()
    {
        if (!registered)
        {
            return;
        }

        navigation.Unregister(this);
        registered = false;
    }

    public void OnNavMeshUpdated(NavMeshUpdate navMeshUpdate)
    {
        if (!MSettings.ExtendDemolishValue)
        {
            return;
        }

        if (bounds.Intersects(navMeshUpdate.Bounds))
        {
            UpdateAccesses();
        }
    }

    public void UpdateAccesses()
    {
        if (!accessible)
        {
            return;
        }

        var z = blockObject.CoordinatesAtBaseZ.z;
        var minZ = ModUtils.GetMinZ(z);
        var maxZ = ModUtils.GetMaxZ(z, mapSize.TotalSize.z);

        var updatedBounds = blockObjectAccessGenerator.GenerateAccessBounds(minZ, maxZ);
        if (registered && !bounds.Equals(updatedBounds))
        {
            navigation.UpdateBounds(this, updatedBounds);
        }

        bounds = updatedBounds;
        accessible.SetAccesses(blockObjectAccessGenerator.GenerateAccesses(minZ, maxZ));
        // Keep disabled so vanilla GetEnabledComponent<Accessible>() still resolves uniquely.
        accessible.DisableComponent();
    }
}
