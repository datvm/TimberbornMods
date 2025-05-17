namespace Ziporter.Components;
partial class ZiporterStabilizer
{
    static readonly Color ExplosionColor = new(159f / 255, 254f / 255, 10f / 255);
    const float ExplosionDuration = 5f;
    const int ExplosionRadius = 4;

    public bool IsExploding { get; private set; }

    public void Explode()
    {
        IsExploding = true;

        ShakeCamera();

        var obj = GetComponentFast<BlockObject>();
        var center = GetExplosionCenter(obj);
        CreateExplosionLight();

        var cells = EnumerateExplosionBoundary(center, ExplosionRadius).ToImmutableArray();

        var objs = cells
        .SelectMany(cell => blockService.GetObjectsAt(cell))
            .ToHashSet();
        foreach (var destroyingObj in objs)
        {
            if (destroyingObj)
            {
                entityService.Delete(destroyingObj);
            }
        }

        foreach (var cell in cells)
        {
            terrainDestroyer.DestroyTerrain(cell);
        }
    }

    void ShakeCamera()
    {
        cameraShakeService
            .MoveTo(GetComponentFast<SelectableObject>())
            .Shake(2.5f, 5f);
    }

    void CreateExplosionLight()
    {
        sunLightOverrider.OverrideSunLight(new(ExplosionColor, ExplosionDuration));
    }

    static Vector3 GetExplosionCenter(BlockObject obj)
    {
        Vector3Int min = obj.Coordinates, max = obj.Coordinates;

        foreach (var b in obj.PositionedBlocks.GetAllBlocks())
        {
            var coord = b.Coordinates;

            for (int i = 0; i < 3; i++)
            {
                if (min[i] > coord[i])
                {
                    min[i] = coord[i];
                }

                if (max[i] < coord[i])
                {
                    max[i] = coord[i];
                }
            }
        }

        // Z is at base level
        var z = obj.Coordinates.z + 1;
        return Vector3.Lerp(
            min with { z = z },
            max with { z = z },
            0.5f);
    }

    IEnumerable<Vector3Int> EnumerateExplosionBoundary(Vector3 center, float radius)
    {
        var rSq = radius * radius;

        int minX = Mathf.FloorToInt(center.x - radius);
        int maxX = Mathf.CeilToInt(center.x + radius);
        int minY = Mathf.FloorToInt(center.y - radius);
        int maxY = Mathf.CeilToInt(center.y + radius);
        int minZ = Mathf.FloorToInt(center.z - radius);
        int maxZ = Mathf.CeilToInt(center.z + radius);

        var size = mapIndexService.TerrainSize;

        for (int x = minX; x <= maxX; x++)
        {
            if (x < 0 || x >= size.x) { continue; }

            for (int y = minY; y <= maxY; y++)
            {
                if (y < 0 || y >= size.y) { continue; }

                for (int z = minZ; z <= maxZ; z++)
                {
                    if (z < 0 || z >= size.z) { continue; }

                    Vector3 diff = new Vector3(x + 0.5f, y + 0.5f, z + 0.5f) - center;
                    if (diff.sqrMagnitude <= rSq)
                    {
                        yield return new Vector3Int(x, y, z);
                    }
                }
            }
        }
    }

}
