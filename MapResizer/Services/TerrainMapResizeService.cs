
namespace MapResizer.Services;

public class TerrainMapResizeService(
    TerrainMap terrainMap,
    MapSize mapSize
)
{
    Vector3Int terrainSizePadding = new(2, 2, 0);

    public void ResizeTerrainMap(in ResizeData oldData, EnlargeStrategy strat)
    {
        var (oldMapSize, _) = oldData;

        var oldTerrainSize = oldMapSize.TerrainSize;
        var newTerrainSize = mapSize.TerrainSize;

        var oldTerrainVoxels = new FlatArrayHelper<bool>(terrainMap._terrainVoxels, oldTerrainSize + terrainSizePadding)
            .PrintDebug(@"D:\Temp\voxel-old.txt")
            .RemovePadding();
        var newTerrainVoxels = new FlatArrayHelper<bool>(newTerrainSize);

        int x0 = oldTerrainSize.x, y0 = oldTerrainSize.y, z0 = oldTerrainSize.z;
        int x1 = newTerrainSize.x, y1 = newTerrainSize.y, z1 = newTerrainSize.z;

        for (int y = 0; y < y1; y++)
        {
            var srcY = GetEnlargeCoord(y0, y, strat);

            for (int x = 0; x < x1; x++)
            {
                var srcX = GetEnlargeCoord(x0, x, strat);

                for (int z = 0; z < z1; z++)
                {
                    newTerrainVoxels[x,y,z] =
                        z < z0
                        && oldTerrainVoxels[srcX, srcY, z];
                }
            }
        }

        terrainMap._terrainVoxels = newTerrainVoxels.AddPadding()
            .PrintDebug(@"D:\Temp\voxel-new.txt")
            .Array;
    }

    static int GetEnlargeCoord(int oldSize, int x, EnlargeStrategy strat)
    {
        if (oldSize == 1) { return 0; }

        if (x < oldSize) { return x; }

        switch (strat)
        {
            case EnlargeStrategy.Mirror:
                {
                    int period = 2 * oldSize;
                    int mod = x % period;
                    return (mod < oldSize) ? mod : period - mod - 1;
                }

            default:
                return oldSize - 1;
        }
    }

}
