

namespace TheArchitectsToolkit.Services.Copier;

public class BlockObjectCopierService(
    MapSize mapSize,
    IBlockService blockService,
    NaturalResourceSpawner naturalResourceSpawner
)
{
    public void Copy(in Vector2Int oldSize)
    {
        int x0 = oldSize.x, y0 = oldSize.y;

        var newSize = mapSize.TotalSize;
        int x1 = newSize.x, y1 = newSize.y, z1 = newSize.z;

        HashSet<BlockObject> copiedObjs = [];
        for (int x = 0; x < x1; x++)
        {
            var srcX = GetEnlargeCoord(x0, x);

            for (int y = 0; y < y1; y++)
            {
                var srcY = GetEnlargeCoord(y0, y);

                for (int z = 0; z < z1; z++)
                {
                    var blockObjs = blockService.GetObjectsAt(new(srcX, srcY, z));

                    foreach (var obj in blockObjs)
                    {
                        if (!copiedObjs.Contains(obj) &&
                            TryPlacingObject(obj, new(x, y, z)))
                        {
                            copiedObjs.Add(obj);
                        }
                    }
                }
            }
        }
    }

    bool TryPlacingObject(BlockObject obj, in Vector3Int location)
    {
        var spec = obj.GetComponentFast<PrefabSpec>();
        if (!spec) { return false; }

        Debug.Log($"Copying {spec.name} to {location}");

        if (TryPlacingNaturalResources(obj, spec, location)) { return true; }

        Debug.LogWarning("Copier does not know how to copy " + spec.name);
        return false;
    }

    bool TryPlacingNaturalResources(BlockObject blockObject, PrefabSpec prefab, in Vector3Int location)
    {
        if (!blockObject.GetComponentFast<NaturalResourceSpec>()) { return false; }

        var isMature = blockObject.GetComponentFast<Growable>().IsGrown;
        naturalResourceSpawner.Spawn([
            new(prefab.PrefabName, !isMature)],
            location);

        return true;
    }

    static int GetEnlargeCoord(int oldSize, int x)
    {
        if (oldSize == 1) { return 0; }

        if (x < oldSize) { return x; }

        var period = 2 * oldSize;
        var mod = x % period;
        return (mod < oldSize) ? mod : period - mod - 1;
    }
}
