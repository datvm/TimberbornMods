namespace TImprove4UX.Services;

public class DynamiteDestructionService(
    EventBus eb,
    MSettings s,
    IBlockService blockService,
    ITerrainPhysicsService terrainPhysicsService,
    TerrainHighlightingService terrainHighlightingService,
    RollingHighlighter highlighter,
    MapSize mapSize,
    ITerrainService terrainService
) : ILoadableSingleton
{

    readonly HashSet<BlockObject> destroyingBos = [];
    readonly HashSet<Vector3Int> destroyingTerrains = [];

    public bool IsEnabled => s.ShowDynamiteDestruction.Value && !MStarter.HasDirectionalDynamite;

    public void Load()
    {
        s.ShowDynamiteDestruction.ValueChanged += (_, _) => OnSettingChanged();
        OnSettingChanged();
    }

    void OnSettingChanged()
    {
        if (IsEnabled)
        {
            eb.Register(this);
        }
        else
        {
            eb.Unregister(this);
            UnhighlightAll();
        }
    }

    public void HighlightDestruction(Dynamite dynamite)
    {
        var coord = dynamite._blockObject.Coordinates.Below();
        var depth = dynamite.CalculateEffectiveDepth(coord);
        
        var terrains = new List<Vector3Int>();
        for (int i = 0; i < depth; i++)
        {            
            if (!mapSize.ContainsInTotal(coord) || !terrainService.Underground(coord)) { break; }
            terrains.Add(coord);
            coord = coord.Below();
        }

        var (dBuildings, dTerrains) = QueryDestructingEntities(terrains);

        highlighter.HighlightPrimary(dBuildings, BrushColors.Negative);
        terrainHighlightingService.UpdateHighlights(dTerrains, BrushColors.Negative);
    }

    [OnEvent]
    public void OnEntitySelected(SelectableObjectSelectedEvent e)
    {
        var dynamite = e.SelectableObject.GetComponentFast<Dynamite>();
        if (dynamite) { HighlightDestruction(dynamite); }
    }

    [OnEvent]
    public void OnEntityUnselected(SelectableObjectUnselectedEvent _) => UnhighlightAll();

    DestroyingEntities QueryDestructingEntities(IEnumerable<Vector3Int> terrains)
    {
        var objs = GetDependentObjectsAboveTerrains(terrains);
        terrainPhysicsService.GetTerrainAndBlockObjectStack(terrains, objs, destroyingTerrains, destroyingBos);
        destroyingTerrains.AddRange(terrains);
        destroyingBos.AddRange(objs);

        return ReturnQuery();
    }

    IEnumerable<BlockObject> GetDependentObjectsAboveTerrains(IEnumerable<Vector3Int> terrains)
    {
        foreach (var c in terrains)
        {
            var blockCoord = c.Above(); // Unity forward is up for the game
            if (!blockService.AnyObjectAt(blockCoord)) { continue; }

            foreach (var bo in blockService.GetObjectsAt(blockCoord).ToArray())
            {
                var matterBelow = bo.PositionedBlocks.GetBlock(blockCoord).MatterBelow;

                if (matterBelow is MatterBelow.Ground or MatterBelow.GroundOrStackable)
                {
                    yield return bo;
                }
            }
        }
    }

    void UnhighlightAll()
    {
        highlighter.UnhighlightAllPrimary();
        terrainHighlightingService.ClearHighlight();
    }

    DestroyingEntities ReturnQuery()
    {
        DestroyingEntities result = new([.. destroyingBos], [.. destroyingTerrains]);
        destroyingBos.Clear();
        destroyingTerrains.Clear();
        return result;
    }

}

public readonly record struct DestroyingEntities(
    ImmutableArray<BlockObject> BlockObjects,
    ImmutableArray<Vector3Int> Terrains
);