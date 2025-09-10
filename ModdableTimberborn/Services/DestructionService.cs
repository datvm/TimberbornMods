namespace ModdableTimberborn.Services;

/// <summary>
/// A service for querying, highlighting and destroying block objects and terrains that would be affected by destruction.
/// </summary>
public class DestructionService(
    ITerrainPhysicsService terrainPhysicsService,
    EntityService entityService,
    TerrainDestroyer terrainDestroyer,
    IBlockService blockService,
    TerrainHighlightingService terrainHighlightingService,
    RollingHighlighter highlighter
)
{

    readonly HashSet<BlockObject> destroyingBos = [];
    readonly HashSet<Vector3Int> destroyingTerrains = [];

    /// <summary>
    /// Queries all block objects and terrains that would be affected by destruction starting from the given terrains.
    /// </summary>
    public DestroyingEntities QueryDestructingEntities(IEnumerable<Vector3Int> terrains)
    {
        var objs = GetDependentObjectsAboveTerrains(terrains);
        terrainPhysicsService.GetTerrainAndBlockObjectStack(terrains, objs, destroyingTerrains, destroyingBos);
        destroyingTerrains.AddRange(terrains);
        destroyingBos.AddRange(objs);

        return ReturnQuery();
    }

    /// <summary>
    /// Finds all block objects directly above the given terrains that depend on them for support.
    /// </summary>
    public IEnumerable<BlockObject> GetDependentObjectsAboveTerrains(IEnumerable<Vector3Int> terrains)
    {
        foreach (var c in terrains)
        {
            var blockCoord = c.Above();
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

    /// <summary>
    /// Queries all block objects and terrains that would be affected by destruction starting from the given block objects.
    /// </summary>
    public DestroyingEntities QueryDestructingEntities(IEnumerable<BlockObject> blockObjects)
    {
        terrainPhysicsService.GetTerrainAndBlockObjectStack(blockObjects, destroyingTerrains, destroyingBos);
        destroyingBos.AddRange(blockObjects);

        return ReturnQuery();
    }

    /// <summary>
    /// Destroys the given block objects and terrains.
    /// </summary>
    public void DestroyEntities(DestroyingEntities entities)
    {
        foreach (var bo in entities.BlockObjects)
        {
            entityService.Delete(bo);
        }
        foreach (var pos in entities.Terrains)
        {
            terrainDestroyer.DestroyTerrain(pos);
        }
    }

    /// <summary>
    /// Highlights the given block objects and terrains in a negative color (red by default).
    /// </summary>
    public void HighlightDestructionEntities(DestroyingEntities entities)
        => HighlightDestructionEntities(entities, BrushColors.Negative);

    /// <summary>
    /// Highlights the given block objects and terrains in the specified color.
    /// </summary>
    public void HighlightDestructionEntities(DestroyingEntities entities, Color color)
    {
        highlighter.HighlightPrimary(entities.BlockObjects, color);
        terrainHighlightingService.UpdateHighlights(entities.Terrains, color);
    }

    /// <summary>
    /// Removes all destruction highlights.
    /// </summary>
    public void UnhighlightDestructionEntities()
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

/// <summary>
/// A record of block objects and terrains that would be affected by destruction.
/// </summary>
/// <param name="BlockObjects">The block objects that would be affected by destruction.</param>
/// <param name="Terrains">The terrains that would be affected by destruction.</param>
public readonly record struct DestroyingEntities(
    ImmutableArray<BlockObject> BlockObjects,
    ImmutableArray<Vector3Int> Terrains
);