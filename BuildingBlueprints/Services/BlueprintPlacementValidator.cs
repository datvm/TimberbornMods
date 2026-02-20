namespace BuildingBlueprints.Services;

[BindSingleton]
public class BlueprintPlacementValidator(
    IEnumerable<IBlockObjectPlacer> placers,
    EntityService entityService,
    IBlockService blockService
)
{
    readonly BuildingPlacer buildingPlacer = (BuildingPlacer)placers.First(p => p is BuildingPlacer);

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
        TaskCompletionSource<BaseComponent> tcs = new();
        buildingPlacer.Place(bos, placement, result => tcs.SetResult(result));
        return tcs.Task;
    }

    bool AlreadyHasPath(Vector3Int position) =>
        // Path is valid if there is already a path there
        blockService.GetPathObjectAt(position);

}
