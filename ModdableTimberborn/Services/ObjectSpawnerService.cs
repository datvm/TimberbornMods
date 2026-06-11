namespace ModdableTimberborn.Services;

[BindSingleton(Contexts = BindAttributeContext.NonMenu)]
public class ObjectSpawnerService(
    BlockValidator blockValidator,
    BlockObjectPlacerService blockObjectPlacerService,
    DestructionService destructionService,
    IBlockService blockService,
    TerrainMap terrainMap,
    TemplateNameMapper templateNameMapper
)
{

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

    public bool IsPlacementValid(BlockObjectSpec template, Placement placement)
        => blockValidator.BlocksValid(template, placement);

    public async Task<BaseComponent> PlaceObject(BlockObjectSpec template, Placement placement)
    {
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
        
        return await tcs.Task;
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

}
