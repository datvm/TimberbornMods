
namespace MapResizer.Services;

public class PlantingMapResizer(
    PlantingService plantingService,
    MapSize mapSize
)
{

    public void Resize()
    {
        ResizePlantingMap();
    }

    void ResizePlantingMap()
    {
        var size = mapSize.TerrainSize;

        var old = plantingService._plantingMap;
        var oldSize = old._size;
        var oldResourceIds = old._resourceIds;

        var resourceIds = new string[size.x, size.y, size.z];

        var maxX = Math.Min(size.x, oldSize.x);
        var maxY = Math.Min(size.y, oldSize.y);
        var maxZ = Math.Min(size.z, oldSize.z);
        for (int x = 0; x < maxX; x++)
        {
            for (int y = 0; y < maxY; y++)
            {
                for (int z = 0; z < maxZ; z++)
                {
                    resourceIds[x, y, z] = oldResourceIds[x, y, z];
                }
            }
        }
    }

}
