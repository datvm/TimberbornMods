namespace MapTransformer.Transform;

[BindSingleton(Contexts = BindAttributeContext.NonMenu)]
class MapTransformService(
    MapSize mapSize,
    MapIndexService mapIndexService,
    Ticker ticker,
    TickOnlyArrayService tickOnlyArrayService,
    IMapTransformSaver saver,
    IEnumerable<IMapResizeComponent> components
)
{
    public bool TryMapPlacement(MapTransform transform, BlockObject obj, out Placement dest)
        => transform.TryMapPlacement(obj.Placement, obj.Blocks.Size, out dest);

    public IEnumerable<BlockObject> GetInvalidBlockObjects(MapTransform transform, EntityRegistry entities)
    {
        foreach (var entity in entities.Entities)
        {
            var obj = entity.GetComponent<BlockObject>();
            if (!obj)
            {
                continue;
            }

            if (!transform.TryMapPlacement(obj.Placement, obj.Blocks.Size, out _))
            {
                yield return obj;
            }
        }
    }

    public void DeleteBlockObjects(IEnumerable<BlockObject> objects, EntityService entityService)
    {
        foreach (var obj in objects)
        {
            entityService.Delete(obj);
        }
    }

    public async Task<IMapTransformSaveReference> PerformAsync(MapTransform transform)
    {
        ticker.FinishFullTick();
        Transforming.Active = true;
        tickOnlyArrayService._isLoadPhase = true;
        try
        {
            var oldMapSize = SnapshotMapSize();
            var oldMapIndex = new MapIndexService(oldMapSize);
            oldMapIndex.Load();

            ApplyMapSize(transform);
            mapIndexService.Load();

            MapTransformContext ctx = new(transform, oldMapSize, oldMapIndex, mapSize, mapIndexService);
            foreach (var component in components.OrderBy(c => c.Order))
            {
                component.Transform(in ctx);
            }

            return await saver.SaveAsync();
        }
        finally
        {
            tickOnlyArrayService._isLoadPhase = false;
            Transforming.Active = false;
        }
    }

    public void Load(IMapTransformSaveReference saveReference) => saver.Load(saveReference);

    MapSize SnapshotMapSize()
    {
        return new MapSize(null!, null!)
        {
            TerrainSize = mapSize.TerrainSize,
            TotalSize = mapSize.TotalSize,
            TerrainSize2D = mapSize.TerrainSize2D,
        };
    }

    void ApplyMapSize(MapTransform transform)
    {
        var newTerrain = transform.NewTerrainSize;
        mapSize._mapSizeSpec = mapSize._mapSizeSpec with
        {
            MaxGameTerrainHeight = newTerrain.z - 1,
            MaxMapEditorTerrainHeight = newTerrain.z - 1,
            MaxHeightAboveTerrain = transform.NewTotalSize.z - newTerrain.z,
        };
        mapSize.Initialize(new Vector2Int(newTerrain.x, newTerrain.y));
    }
}
