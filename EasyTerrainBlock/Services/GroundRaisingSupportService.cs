namespace EasyTerrainBlock.Services;

[BindSingleton]
public class GroundRaisingSupportService(
    IBlockService blockService,
    DestructionService destructionService,
    Highlighter highlighter,
    TerrainHighlightingService terrainHighlightingService,
    TerrainPhysicsPostLoader terrainPhysicsPostLoader,
    EntityService entityService
) : ILoadableSingleton, IUnloadableSingleton
{

    public static GroundRaisingSupportService? Instance { get; private set; }

    readonly HashSet<Vector3Int> highlightingTerrains = [];

    public async void ClearObjectsAt(GroundRaiser raiser)
    {
        var terrainBo = raiser.GetComponent<BlockObject>();

        var destroying = blockService.GetObjectsAt(terrainBo.Coordinates).Where(bo => bo != terrainBo).ToArray();
        foreach (var bo in destroying)
        {
            entityService.Delete(bo);
        }

        await Awaitable.NextFrameAsync();
        terrainPhysicsPostLoader.ValidateAll();
    }

    public void HighlightDestroyingObjectsAt(Vector3Int coords)
    {
        var destroying = blockService.GetObjectsAt(coords);
        var query = destructionService.QueryDestructingEntities(destroying);

        // Don't use this, it does not roll over
        // destructionService.HighlightDestructionEntities(query);

        highlightingTerrains.UnionWith(query.Terrains);
        terrainHighlightingService.UpdateHighlight(highlightingTerrains);

        foreach (var e in query.BlockObjects)
        {
            highlighter.HighlightPrimary(e, TimberUiUtils.DangerColor);
        }
    }

    public void UnhighlightDestruction()
    {
        highlightingTerrains.Clear();
        highlighter.UnhighlightAllPrimary();
        terrainHighlightingService.ClearHighlight();
    }

    public void Load() => Instance = this;
    public void Unload() => Instance = null;
}
