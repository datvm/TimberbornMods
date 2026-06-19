namespace BuildingBlueprints.Services;

#if TIMBERV11
using Timberborn.ConstructionSites;
#endif

[BindSingleton]
public class BlueprintPlacementValidator(
    IEnumerable<IBlockObjectPlacer> placers,
    EntityService entityService,
    IBlockService blockService
#if TIMBERV11
    // 1.1: BuildingPlacer.Place lost its result callback; place finished buildings via the factory instead.
    , ConstructionFactory constructionFactory
#endif
)
{
#if !TIMBERV11
    readonly BuildingPlacer buildingPlacer = (BuildingPlacer)placers.First(p => p is BuildingPlacer);
#endif

    public async Task<HashSet<Preview>> ValidatePreviewsAsync(ImmutableArray<Preview> previews) 
        => (await ValidatePreviews(previews, true)).ValidPreviews;
    public async Task<List<ValueTuple<Preview, BaseComponent>>> BuildPreviewsAsync(ImmutableArray<Preview> previews) 
        => (await ValidatePreviews(previews, false)).PlacedBuildings;

    async Task<(HashSet<Preview> ValidPreviews, List<ValueTuple<Preview, BaseComponent>> PlacedBuildings)> ValidatePreviews(ImmutableArray<Preview> previews, bool destroy)
    {
        HashSet<Preview> result = [];

        // Greedily sort by Z
        Preview[] sorted = [.. previews.OrderBy(p => p.BlockObject.Coordinates.z)];

        var count = sorted.Length;
        List<ValueTuple<Preview, BaseComponent>> placed = [];
        var validCount = 0;

        while (validCount < count)
        {
            var hasNew = false;

            foreach (var p in sorted)
            {
                if (result.Contains(p)) { continue; }

                var (valid, comp) = await TryPlacingAsync(p);
                if (!valid) { continue; }

                validCount++;
                hasNew = true;
                if (comp is not null)
                {
                    placed.Add((p, comp));
                }
                
                result.Add(p);
            }

            if (!hasNew)
            {
                break;
            }
        }

        if (destroy)
        {
            foreach (var (_, p) in placed)
            {
                // Disable the deconstructible so effects don't trigger
                p.GetComponent<Deconstructible>()?.DisableComponent();

                entityService.Delete(p);
            }
            placed.Clear();
        }

        return (result, placed);
    }

    async Task<(bool, BaseComponent?)> TryPlacingAsync(Preview preview)
    {
        var bo = preview.BlockObject;
        
        var template = bo.GetComponent<TemplateSpec>().TemplateName;
        switch (template)
        {
            case "Path" when AlreadyHasPath(bo.Coordinates):
                return (true, null);
        }

        if (!bo.IsValid()) { return (false, null); }

        var bos = bo.GetComponent<BlockObjectSpec>();
        var placement = bo.Placement;

        return (true, await PlaceAsync(bos, placement));
    }

    Task<BaseComponent> PlaceAsync(BlockObjectSpec bos, Placement placement)
    {
#if TIMBERV11
        // Mirror BuildingPlacer's ShouldBePlacedFinished: a normal building is placed UNFINISHED (a
        // construction site beavers build); only specs flagged PlaceFinished spawn already-built.
        // (Forcing CreateAsFinished here made every pasted blueprint instantly complete — wrong.)
        var builder = new EntitySetup.Builder(bos.Blueprint);
        var placed = bos.Blueprint.GetSpec<BuildingSpec>().PlaceFinished
            ? constructionFactory.CreateAsFinished(builder, placement)
            : constructionFactory.CreateAsUnfinished(builder, placement);
        return Task.FromResult<BaseComponent>(placed);
#else
        TaskCompletionSource<BaseComponent> tcs = new();
        buildingPlacer.Place(bos, placement, result => tcs.SetResult(result));
        return tcs.Task;
#endif
    }

    bool AlreadyHasPath(Vector3Int position) =>
        // Path is valid if there is already a path there
        blockService.GetPathObjectAt(position);

}
