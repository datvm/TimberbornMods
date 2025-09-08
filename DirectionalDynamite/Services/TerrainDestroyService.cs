namespace DirectionalDynamite.Services;

public class TerrainDestroyService(
    ITerrainPhysicsService terrainPhysicsService,
    EntityService entityService,
    TerrainDestroyer terrainDestroyer,
    IBlockService blockService
)
{

    readonly HashSet<BlockObject> destroyingBos = [];
    readonly HashSet<Vector3Int> destroyingTerrains = [];

    public DestroyingEntities QueryDestructingEntities(IEnumerable<Vector3Int> terrains)
    {
        var objs = GetDependentObjectsAboveTerrains(terrains);
        terrainPhysicsService.GetTerrainAndBlockObjectStack(terrains, objs, destroyingTerrains, destroyingBos);
        destroyingTerrains.AddRange(terrains);
        destroyingBos.AddRange(objs);

        return ReturnQuery();
    }

    public IEnumerable<BlockObject> GetDependentObjectsAboveTerrains(IEnumerable<Vector3Int> terrains)
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

    public DestroyingEntities QueryDestructingEntities(IEnumerable<BlockObject> blockObjects)
    {
        terrainPhysicsService.GetTerrainAndBlockObjectStack(blockObjects, destroyingTerrains, destroyingBos);
        destroyingBos.AddRange(blockObjects);

        return ReturnQuery();
    }

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