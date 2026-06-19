namespace ModdableTimberborn.Services;

#if TIMBERV11
using Timberborn.ConstructionSites;
#endif

[BindSingleton(Contexts = BindAttributeContext.NonMenu)]
public class BlockObjectSpawningHelper(
    BlockValidator blockValidator,
    BlockObjectPlacerService blockObjectPlacerService,
    DestructionService destructionService,
    IBlockService blockService,
    TerrainMap terrainMap,
    IThreadSafeColumnTerrainMap columnTerrainMap,
    TemplateNameMapper templateNameMapper,
    EntityRegistry entityRegistry,
    MapIndexService mapIndexService
#if TIMBERV11
    // 1.1: placement returns the entity via the factories instead of the removed Place(..., callback) overload.
    // Buildings go through ConstructionFactory (building-aware finished wiring); other block objects through BlockObjectFactory.
    , BlockObjectFactory blockObjectFactory
    , ConstructionFactory constructionFactory
#endif
)
{
    static readonly ImmutableArray<Orientation> Orientations = TimberUiUtils.GetSortedEnumValues<Orientation>();

    public bool TryGetBlockObjectSpec(string templateName, [NotNullWhen(true)] out BlockObjectSpec? spec, bool throwOnInvalidFoundTemplate = false)
    {
        if (templateNameMapper.TryGetTemplate(templateName, out var template))
        {
            spec = template.GetSpec<BlockObjectSpec>();

            if (spec is null)
            {
                if (throwOnInvalidFoundTemplate)
                {
                    throw new InvalidOperationException($"Template '{templateName}' is not a block object.");
                }

                return false;
            }

            return true;
        }

        spec = null;
        return false;
    }

    public bool TryGetBlockObjectSpec(IEnumerable<string> templateNames, [NotNullWhen(true)] out BlockObjectSpec? spec, bool throwOnInvalidFoundTemplate = false)
    {
        foreach (var name in templateNames)
        {
            if (TryGetBlockObjectSpec(name, out spec, throwOnInvalidFoundTemplate))
            {
                return true;
            }
        }
        spec = null;
        return false;
    }

    public bool IsPlacementValid(BlockObjectSpec template, Placement placement)
        => blockValidator.BlocksValid(template, placement);

    public Task<BaseComponent> PlaceObject(BlockObjectSpec template, Placement placement)
    {
#if TIMBERV11
        // 1.1 reworked placement: the placer's Place() lost its result callback and now takes an
        // EntitySetup.Builder. Mirror what GetMatchingPlacer did: buildings go through ConstructionFactory
        // following the spec's PlaceFinished flag (normal buildings → unfinished construction site, not
        // instantly built); non-building block objects are placed finished. All paths return the BlockObject.
        var builder = new EntitySetup.Builder(template.Blueprint);
        BaseComponent placed;
        if (template.Blueprint.HasSpec<BuildingSpec>())
        {
            placed = template.Blueprint.GetSpec<BuildingSpec>().PlaceFinished
                ? constructionFactory.CreateAsFinished(builder, placement)
                : constructionFactory.CreateAsUnfinished(builder, placement);
        }
        else
        {
            placed = blockObjectFactory.CreateFinished(builder, placement);
        }
        return Task.FromResult(placed);
#else
        var placer = blockObjectPlacerService.GetMatchingPlacer(template);

        TaskCompletionSource<BaseComponent> tcs = new();

        try
        {
            placer.Place(template, placement, c => tcs.TrySetResult(c));
        }
        catch (Exception ex)
        {
            tcs.TrySetException(ex);
        }

        return tcs.Task;
#endif
    }

    public async Task<BaseComponent?> TryPlacingWithDestruction(BlockObjectSpec template, Placement placement)
    {
        if (IsPlacementValid(template, placement))
        {
            return await PlaceObject(template, placement);
        }

        DestroyOccupyingObjects(template, placement);
        return IsPlacementValid(template, placement) ? await PlaceObject(template, placement) : null;
    }

    public bool TryFindPlacement(BlockObjectSpec template, [NotNullWhen(true)] out Placement? placement, PlacementFindingLimits limits = default)
    {
        placement = null;

        var area = FindLimitingArea(limits);
        area.ClampToBounds(GetMapArea());

        var (sx, sy, _) = template.Size;
        var minSize = Math.Min(sx, sy);
        var maxSize = Math.Max(sx, sy);

        if ((area.width < maxSize && area.height < maxSize)
            || area.width < minSize || area.height < minSize) { return false; }

        if (area.width <= 0 || area.height <= 0)
        {
            return false;
        }

        var startX = Random.RandomRangeInt(area.x, area.xMax);
        var startY = Random.RandomRangeInt(area.y, area.yMax);
        var start = new Vector2Int(startX, startY);
        var maxRadius = Math.Max(startX - area.x, Math.Max(area.xMax - startX, Math.Max(startY - area.y, area.yMax - startY)));

        var columns = columnTerrainMap.TerrainColumns;
        var columnCounts = columnTerrainMap.ColumnCounts;
        var stride = mapIndexService.VerticalStride;

        foreach (var delta in NeighbourFinder.GetSpiralNeighboursXY(maxRadius))
        {
            var curr = start + delta;
            if (!area.Contains(curr)) { continue; }

            var index2D = mapIndexService.CellToIndex(curr);
            var columnCount = columnCounts[index2D];

            for (int z = 0; z < columnCount; z++)
            {
                var index3D = index2D + z * stride;
                var column = columns[index3D];
                var coord = new Vector3Int(curr.x, curr.y, column.Ceiling);

                foreach (var ori in Orientations)
                {
                    var currPlacement = new Placement(coord, ori, FlipMode.Unflipped);
                    if (IsPlacementValid(template, currPlacement))
                    {
                        placement = currPlacement;
                        return true;
                    }
                }
            }
        }

        return false;
    }

    RectInt FindLimitingArea(PlacementFindingLimits limits)
    {
        if (limits == default) { return GetMapArea(); }

        if (limits.LimitedArea.HasValue)
        {
            return limits.LimitedArea.Value;
        }

        var building = FindRandomBuildings();
        if (building is null) { return GetMapArea(); }

        var radius = limits.WithinBuildingRadius;
        var start = building.Coordinates;
        return new(start.x - radius, start.y - radius, radius * 2 + 1, radius * 2 + 1);
    }

    RectInt GetMapArea()
    {
        var (sx, sy, _) = mapIndexService.TerrainSize;
        return new(0, 0, sx, sy);
    }

    public void DestroyOccupyingObjects(BlockObjectSpec template, Placement placement)
    {
        HashSet<Vector3Int> terrains = [];
        HashSet<BlockObject> deletingObjs = [];

        var blocks = template.GetBlocks(placement);
        foreach (var b in blocks)
        {
            var occ = b.Occupation;
            if (occ == BlockOccupations.None) { continue; }

            var coords = b.Coordinates;

            if (terrainMap.IsTerrainVoxel(coords))
            {
                terrains.Add(coords);
                continue;
            }

            foreach (var obj in blockService.GetObjectsAt(coords))
            {
                if (obj.PositionedBlocks.GetAllBlocks().Any(ob => ob.Coordinates == coords && (ob.Occupation & occ) != 0))
                {
                    deletingObjs.Add(obj);
                }
            }
        }

        var result1 = destructionService.QueryDestructingEntities(deletingObjs);
        var result2 = destructionService.QueryDestructingEntities(terrains);

        var resultBos = result1.BlockObjects.Concat(result2.BlockObjects).Distinct();
        var resultTerrains = result1.Terrains.Concat(result2.Terrains).Distinct();

        var combine = new DestroyingEntities([.. resultBos], [.. resultTerrains]);
        destructionService.DestroyEntities(combine);
    }

    public BlockObject? FindRandomBuildings()
    {
        var list = entityRegistry.Entities;
        var count = list.Count;

        if (count == 0) { return null; }

        var start = Random.RandomRangeInt(0, count);

        for (var offset = 0; offset < count; offset++)
        {
            var i = start + offset;

            if (i >= count) { i -= count; }

            var bo = list[i].GetComponent<BlockObject>();
            if (bo && bo.HasComponent<BuildingSpec>()) { return bo; }
        }

        return null;
    }

}

public readonly record struct PlacementFindingLimits(
    int WithinBuildingRadius = 0,
    RectInt? LimitedArea = null
);