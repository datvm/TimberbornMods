namespace MapTransformer.Components;

[MultiBind(typeof(IMapResizeComponent), Contexts = BindAttributeContext.NonMenu)]
class TerrainMapTransformComponent(TerrainMap terrainMap) : IMapResizeComponent
{
    public int Order => 10;

    public void Transform(in MapTransformContext ctx)
    {
        var oldVoxels = terrainMap._terrainVoxels;
        var newVoxels = new bool[ctx.MapIndex.MaxSize3D];
        MapIndexCopy.CopyTerrainVoxels(oldVoxels, newVoxels, in ctx);
        FillAddedDirt(newVoxels, in ctx);
        terrainMap._terrainVoxels = newVoxels;
    }

    static void FillAddedDirt(bool[] newVoxels, in MapTransformContext ctx)
    {
        var t = ctx.Transform;
        if (t.Op != MapOp.AddHeight)
        {
            return;
        }

        var size = t.NewTerrainSize;
        var layers = t.InsertLayers;
        for (var y = 0; y < size.y; y++)
        {
            for (var x = 0; x < size.x; x++)
            {
                for (var z = 0; z < layers && z < size.z - 1; z++)
                {
                    newVoxels[ctx.MapIndex.CoordinatesToIndex3D(new Vector3Int(x, y, z))] = true;
                }
            }
        }
    }
}
