namespace DirectionalDynamite.Services;

public class DirectionalDynamiteService(
    ILoc t,
    IBlockService blockService,
    ITerrainService terrainService,
    TerrainDestroyService terrainDestroyService,
    TerrainHighlightingService terrainHighlightingService,
    RollingHighlighter highlighter,
    MapSize mapSize,
    ISpecService specService
) : ILoadableSingleton
{

    public readonly TerrainDestroyService TerrainDestroyService = terrainDestroyService;
    BrushColorSpec brushColorSpec = null!;

    public static readonly Vector3Int[] DirectionOffsets = [
        new(1, 0, 0), // Down (x++)
        new(0, 1, 0), // Left (y++)
        new(-1, 0, 0), // Up (x--)
        new(0, -1, 0), // Right (y--)
        new(0, 0, -1), // Bottom (z--)
        new(0, 0, 1)  // Top (z++)
    ];

    public static readonly ImmutableArray<Direction3D> AllDirections = [.. Enum.GetValues(typeof(Direction3D))
        .Cast<Direction3D>()
        .OrderBy(q => (int)q)];

    public IReadOnlyList<string> DirectionNames { get; private set; } = null!;

    public void Load()
    {
        brushColorSpec = specService.GetSingleSpec<BrushColorSpec>();
        DirectionNames = [.. AllDirections.Select(d => t.T("LV.DDy.Direction" + d))];
    }

    public ImmutableArray<Vector3Int> GetDestroyingTerrains(DirectionalDynamiteComponent comp)
    {
        var (coord, maxDepth, direction) = comp;

        var encountered = false;
        var currDepth = 0;
        List<Vector3Int> result = [];

        while (currDepth < maxDepth)
        {
            currDepth++;
            coord = GoInDirection(coord, direction);

            if (!mapSize.ContainsInTotal(coord) || blockService.AnyObjectAt(coord)) { break; }

            var hasTerrain = terrainService.Underground(coord);
            if (hasTerrain)
            {
                encountered = true;
                result.Add(coord);
            }
            else
            {
                if (encountered) { break; } // Stop when we reach air after terrain
            }
        }

        return [.. result];
    }

    public void HighlightDestroyingEntities(DirectionalDynamiteComponent comp)
    {
        var destroyingTerrain = GetDestroyingTerrains(comp);
        HighlightDestroyingEntities(destroyingTerrain);
    }

    public void HighlightDestroyingEntities(IEnumerable<DirectionalDynamiteComponent> comps)
    {
        var destroyingTerrain = comps.SelectMany(q => GetDestroyingTerrains(q)).Distinct().ToArray();
        HighlightDestroyingEntities(destroyingTerrain);
    }

    void HighlightDestroyingEntities(IEnumerable<Vector3Int> coords)
    {
        var (buildings, terrains) = TerrainDestroyService.QueryDestructingEntities(coords);
        highlighter.HighlightPrimary(buildings, brushColorSpec.Negative);

        terrainHighlightingService.UpdateHighlight(terrains);
    }

    public void UnhighlightDestroyingEntities()
    {
        highlighter.UnhighlightAllPrimary();
        terrainHighlightingService.ClearHighlight();
    }

    public void DestroyTerrains(IReadOnlyList<Vector3Int> coords)
    {
        var entities = TerrainDestroyService.QueryDestructingEntities(coords);
        TerrainDestroyService.DestroyEntities(entities);
    }

    public static Vector3Int GoInDirection(Vector3Int src, Direction3D direction)
        => src + DirectionOffsets[(int)direction];

}
