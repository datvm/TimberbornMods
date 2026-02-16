namespace BuildingBlueprints.Services;

[BindSingleton]
public class BlueprintPlacementValidator(
    IEnumerable<IBlockObjectPlacer> placers,
    EntityService entityService
)
{
    readonly BuildingPlacer buildingPlacer = (BuildingPlacer)placers.First(p => p is BuildingPlacer);

    public async Task<HashSet<Preview>> ValidatePreviewsAsync(IEnumerable<Preview> previews) 
        => (await ValidatePreviews(previews, true)).ValidPreviews;
    public async Task<List<BaseComponent>> BuildPreviewsAsync(IEnumerable<Preview> previews) 
        => (await ValidatePreviews(previews, false)).PlacedBuildings;

    async Task<(HashSet<Preview> ValidPreviews, List<BaseComponent> PlacedBuildings)> ValidatePreviews(IEnumerable<Preview> previews, bool destroy)
    {
        HashSet<Preview> result = [];

        // Greedily sort by Z
        Preview[] sorted = [.. previews.OrderBy(p => p.BlockObject.Coordinates.z)];

        var count = sorted.Length;
        List<BaseComponent> placed = [];

        while (placed.Count < count)
        {
            var hasNew = false;

            foreach (var p in sorted)
            {
                if (result.Contains(p)) { continue; }

                var comp = await TryPlacingAsync(p);
                if (comp is null) { continue; }

                hasNew = true;
                placed.Add(comp);
                result.Add(p);
            }

            if (!hasNew)
            {
                break;
            }
        }

        if (destroy)
        {
            foreach (var p in placed)
            {
                // Disable the deconstructible so effects don't trigger
                p.GetComponent<Deconstructible>()?.DisableComponent();

                entityService.Delete(p);
            }
            placed.Clear();
        }

        return (result, placed);
    }

    static readonly Task<BaseComponent?> NullTask = Task.FromResult<BaseComponent?>(null);
    Task<BaseComponent?> TryPlacingAsync(Preview preview)
    {
        var bo = preview.BlockObject;
        if (!bo.IsValid()) { return NullTask; }

        var bos = bo.GetComponent<BlockObjectSpec>();
        var placement = bo.Placement;

        return PlaceAsync(bos, placement);
    }

    Task<BaseComponent?> PlaceAsync(BlockObjectSpec bos, Placement placement)
    {
        TaskCompletionSource<BaseComponent?> tcs = new();
        buildingPlacer.Place(bos, placement, result => tcs.SetResult(result));
        return tcs.Task;
    }



}
