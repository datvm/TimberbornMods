namespace MapTransformer.Components;

[MultiBind(typeof(IMapResizeComponent), Contexts = BindAttributeContext.NonMenu)]
class PlantingTransformComponent(PlantingService plantingService, MapSize mapSize) : IMapResizeComponent
{
    public int Order => 70;

    public void Transform(in MapTransformContext ctx)
    {
        var old = plantingService._plantingMap;
        var neu = new PlantingMap(mapSize.TerrainSize);
        var size = mapSize.TerrainSize;
        for (var y = 0; y < size.y; y++)
        {
            for (var x = 0; x < size.x; x++)
            {
                for (var z = 0; z < size.z; z++)
                {
                    var dest = new Vector3Int(x, y, z);
                    if (!ctx.Transform.TryUnmapOrFill(dest, out var src))
                    {
                        continue;
                    }

                    var resource = old.GetResource(src);
                    if (resource is not null)
                    {
                        neu.SetResource(dest, resource);
                    }
                }
            }
        }

        plantingService._plantingMap = neu;
    }
}
