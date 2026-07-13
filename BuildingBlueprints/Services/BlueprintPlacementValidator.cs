namespace BuildingBlueprints.Services;

[BindSingleton]
public class BlueprintPlacementValidator(
    EntityService entityService,
    IBlockService blockService,
    BlockObjectSpawningHelper spawningHelper
)
{
    public HashSet<Preview> ValidatePreviews(ImmutableArray<Preview> previews)
        => ValidatePreviews(previews, true).ValidPreviews;
    public List<(Preview, BaseComponent)> BuildPreviews(ImmutableArray<Preview> previews)
        => ValidatePreviews(previews, false).PlacedBuildings;

    (HashSet<Preview> ValidPreviews, List<(Preview, BaseComponent)> PlacedBuildings) ValidatePreviews(ImmutableArray<Preview> previews, bool destroy)
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

                var (valid, comp) = TryPlacing(p);
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

    (bool, BaseComponent?) TryPlacing(Preview preview)
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

        return (true, spawningHelper.PlaceObject(bos, placement));
    }


    bool AlreadyHasPath(Vector3Int position) =>
        // Path is valid if there is already a path there
        blockService.GetPathObjectAt(position);

}
