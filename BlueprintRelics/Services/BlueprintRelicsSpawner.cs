namespace BlueprintRelics.Services;

public class BlueprintRelicsSpawner(
    EventBus eb,
    ISpecService specs,
    BlueprintRelicsRegistry registry,
    TemplateNameMapper templateMapper,
    PreviewFactory previewFactory,
    BlockObjectPlacerService blockObjectPlacerService,
    IThreadSafeColumnTerrainMap terrainMap,
    TerrainMap rawTerrainMap,
    MapIndexService mapIndexService,
    DistrictCenterRegistry districtCenterRegistry,
    ILoc t,
    NotificationBus notfBus,
    BlockService blockService,
    GameUISoundController sounds
) : ILoadableSingleton
{
    const int MaxStartingPositionDelta = 20;
    static readonly ImmutableArray<string> TemplateNames = ["SmallBlueprintRelic", "MediumBlueprintRelic", "LargeBlueprintRelic"];

    public Vector2Int MapSize => mapIndexService.TerrainSize.XY();


#nullable disable
    BlueprintRelicSpawnerSpec spec;
    IBlockObjectPlacer placer;
#nullable enable

    ImmutableArray<RelicPlacementReferences> relicTemplates = [];

    public void Load()
    {
        spec = specs.GetSingleSpec<BlueprintRelicSpawnerSpec>();
        relicTemplates = [.. TemplateNames.Select(name => {
            var template = templateMapper.GetTemplate(name);
            var placeable = template.GetSpec<PlaceableBlockObjectSpec>();
            var blockObject = template.GetSpec<BlockObjectSpec>();

            placer ??= blockObjectPlacerService.GetMatchingPlacer(blockObject);
            var preview = previewFactory.Create(placeable);

            return new RelicPlacementReferences(blockObject, preview);
        })];

        eb.Register(this);
    }

    [OnEvent]
    public void OnNewDay(CycleDayStartedEvent _) => TryToSpawn();

    internal void TryToSpawn()
    {
        var count = registry.Count;
        if (count >= spec.MaxRelics) { return; }

        var chance = spec.BaseChance * Mathf.Pow(spec.ChanceMultiplierPerRelic, count);
        if (Random.Range(0f, 1f) > chance) { return; }

        Spawn();
    }

    internal void Spawn()
    {
        var size = GetSizeIndex();
        SpawnRelic(relicTemplates[size]);
    }

    void SpawnRelic(RelicPlacementReferences references)
    {
        var (bo, preview) = references;

        var spot = FindSpawnSpot(preview);
        if (spot is null)
        {
            Debug.LogWarning("[BlueprintRelicsSpawner] Could not find a spawn spot for blueprint relic.");
            return;
        }

        placer.Place(bo, spot.Value, obj =>
        {
            var label = obj.GetComponent<LabeledEntity>();

            var msg = t.T("LV.BRe.SpawnNotif", label.DisplayName);
            notfBus.Post(msg, obj);

            sounds.PlayWellbeingHighscoreSound();
        });
    }

    int GetSizeIndex()
    {
        var weightTotal = spec.SizeChanceWeight.Sum();
        var roll = Random.RandomRangeInt(0, weightTotal);
        var cumulative = 0;

        for (int i = 0; i < spec.SizeChanceWeight.Length; i++)
        {
            cumulative += spec.SizeChanceWeight[i];
            if (roll < cumulative)
            {
                return i;
            }
        }

        throw new InvalidOperationException(); // Should never reach here
    }

    static readonly ImmutableArray<Orientation> AllOrientations = [.. Enum.GetValues(typeof(Orientation)).Cast<Orientation>()];
    Placement? FindSpawnSpot(Preview preview)
    {
        var spot = FindInitSpot();

        // Move randomly
        var (sx, sy) = MapSize;
        spot.x = Math.Clamp(spot.x + Random.RandomRangeInt(-MaxStartingPositionDelta, MaxStartingPositionDelta + 1), 1, sx - 1);
        spot.y = Math.Clamp(spot.y + Random.RandomRangeInt(-MaxStartingPositionDelta, MaxStartingPositionDelta + 1), 1, sy - 1);

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
                .SelectMany(q =>
                {
                    var index3D = index2D + q * stride;
                    var column = columns[index3D];
                    var coord = new Vector3Int(curr.x, curr.y, column.Ceiling);

                    return AllOrientations.Select(orientation => new Placement(coord, orientation, FlipMode.Unflipped));
                });

            var buildables = GetBuildablePlacements(placements, preview).ToArray();
            if (buildables.Length > 0)
            {
                return buildables[buildables.Length == 1 ? 0 : Random.RandomRangeInt(0, buildables.Length)];
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

    IEnumerable<Placement> GetBuildablePlacements(IEnumerable<Placement> placements, Preview preview)
    {
        foreach (var p in placements)
        {
            preview.Reposition(p);
            if (preview.BlockObject.IsValid())
            {
                var entranceBlock = preview.BlockObject.PositionedEntrance.Coordinates;
                if (!blockService.Contains(entranceBlock)
                    || rawTerrainMap.IsTerrainVoxel(entranceBlock)
                    || blockService.AnyNonOverridableObjectsAt(entranceBlock, BlockOccupations.Path))
                {
                    continue;
                }

                yield return p;
            }
        }
    }

    Vector2Int FindInitSpot()
    {
        var buildings = districtCenterRegistry.FinishedDistrictCenters;
        if (buildings.Count > 0)
        {
            return buildings
                .Skip(Random.RandomRangeInt(0, buildings.Count))
                .First()
                .GetComponent<BlockObject>()
                .Coordinates.XY();
        }

        var (sx, sy) = MapSize;
        return new(sx / 2, sy / 2);
    }

    readonly record struct RelicPlacementReferences(BlockObjectSpec BlockObject, Preview Preview);

}
