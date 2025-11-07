namespace ExtendedBuilderReach.Components;

public class ExtendedGoodStackAccessible : BaseComponent, INavMeshListener, IAwakableComponent, IStartableComponent
{
    BoundingBox bounds;

#nullable disable
    GoodStackAccessible goodStackAccessible;
#nullable enable

    public void Awake()
    {
        goodStackAccessible = GetComponent<GoodStackAccessible>();
    }

    public void OnNavMeshUpdated(NavMeshUpdate navMeshUpdate)
    {
        if (bounds.Intersects(navMeshUpdate.Bounds))
        {
            goodStackAccessible.Enable();
        }
    }

    public void Start()
    {
        var bo = GetComponent<BlockObject>();
        var z = bo.CoordinatesAtBaseZ.z;

        var minZ = ModUtils.GetMinZ(z);
        var maxZ = ModUtils.GetMaxZ(z, bo._blockService._mapSize.TotalSize.z);

        bounds = GetComponent<BlockObjectAccessGenerator>().GenerateAccessBounds(minZ, maxZ);
        goodStackAccessible.Enable();
    }
}
