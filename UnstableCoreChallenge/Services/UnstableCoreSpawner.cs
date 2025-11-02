namespace UnstableCoreChallenge.Services;

public class UnstableCoreSpawner(
    DefaultEntityTracker<Building> buildingTracker,
    MapIndexService mapIndexService,
    IThreadSafeColumnTerrainMap terrainMap,
    PreviewFactory previewFactory,
    TemplateService templateService,
    BlockObjectPlacerService blockObjectPlacerService
) : ILoadableSingleton
{
    const int MaxStartingPositionDelta = 5;

    public Vector2Int MapSize => mapIndexService.TerrainSize.XY();

#nullable disable
    PlaceableBlockObjectSpec unstableCorePlacableSpec;
    BlockObjectSpec unstableCoreBlockObjSpec;
    IBlockObjectPlacer blockObjectPlacer;
    Preview preview;
#nullable enable

    public void Load()
    {
        try
        {
            var template = templateService.GetAll<UnstableCoreSpec>()
                .Single(q => q.GetSpec<TemplateSpec>() is { TemplateName: "UnstableCore" });
            unstableCorePlacableSpec = template.GetSpec<PlaceableBlockObjectSpec>();
            unstableCoreBlockObjSpec = template.GetSpec<BlockObjectSpec>();
        }
        catch (Exception ex)
        {
            throw new MissingComponentException("Could not find Unstable Core", ex);
        }

        blockObjectPlacer = blockObjectPlacerService.GetMatchingPlacer(unstableCoreBlockObjSpec);
        preview = previewFactory.Create(unstableCorePlacableSpec);
    }

    public void SpawnCore()
    {
        var placement = FindSpawnSpot();
        if (placement is null) { return; }

        blockObjectPlacer.Place(unstableCoreBlockObjSpec, placement.Value, OnCoreSpawned);
    }

    void OnCoreSpawned(BaseComponent comp)
    {
        var eff = comp.GetComponent<UnstableCoreVisualisation>();
        if (eff)
        {
            eff.OnUnselect();
        }
    }

    Placement? FindSpawnSpot()
    {
        var spot = FindInitSpot();

        // Move randomly
        var (sx, sy) = MapSize;
        spot.x = Math.Clamp(spot.x + UnityEngine.Random.RandomRangeInt(-MaxStartingPositionDelta, MaxStartingPositionDelta + 1), 1, sx - 1);
        spot.y = Math.Clamp(spot.y + UnityEngine.Random.RandomRangeInt(-MaxStartingPositionDelta, MaxStartingPositionDelta + 1), 1, sy - 1);

        HashSet<Vector2Int> checkedSpots = [spot];
        Queue<Vector2Int> spots = new(checkedSpots);


        var columns = terrainMap.TerrainColumns;
        var columnCounts = terrainMap.ColumnCounts;
        var stride = mapIndexService.VerticalStride;

        var neighbors = Deltas.Neighbors4Vector2Int;
        while (spots.Count > 0)
        {
            var curr = spots.Dequeue();
            var index2D = mapIndexService.CellToIndex(curr);
            var columnCount = columnCounts[index2D];

            var placements = Enumerable.Range(0, columnCount)
                .Select(q =>
                {
                    var index3D = index2D + q * stride;
                    var column = columns[index3D];
                    var coord = new Vector3Int(curr.x, curr.y, column.Ceiling);

                    return new Placement(coord);
                });

            var buildables = GetBuildablePlacements(placements).ToArray();
            if (buildables.Length > 0)
            {
                return buildables[buildables.Length == 1 ? 0 : UnityEngine.Random.RandomRangeInt(0, buildables.Length)];
            }

            // Enqueue neighbors
            foreach (var delta in neighbors)
            {
                var target = curr + delta;
                if (!checkedSpots.Contains(target)
                    && target.x >= 1 && target.x < sx - 1
                    && target.y >= 1 && target.y < sy - 1)
                {
                    checkedSpots.Add(target);
                    spots.Enqueue(target);
                }
            }
        }
        return null;
    }

    IEnumerable<Placement> GetBuildablePlacements(IEnumerable<Placement> placements)
    {
        foreach (var p in placements)
        {
            preview.Reposition(p);
            if (preview.BlockObject.IsValid())
            {
                yield return p;
            }
        }
    }

    Vector2Int FindInitSpot()
    {
        var buildings = buildingTracker.Entities;
        if (buildings.Count > 0)
        {
            return buildings
                .Skip(UnityEngine.Random.RandomRangeInt(0, buildings.Count))
                .First()
                .GetComponent<BlockObject>()
                .Coordinates.XY();
        }

        var (sx, sy) = MapSize;
        return new(sx / 2, sy / 2);
    }

}
