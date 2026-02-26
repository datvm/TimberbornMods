namespace ConfigurableFaction.Services.SpecAppenders;

[MultiBind(typeof(ISpecModifier))]
public class FactionSpecModifier : BaseBlueprintModifier<FactionSpec>
{
    public override IEnumerable<EditableBlueprint> Modify(IEnumerable<EditableBlueprint> blueprints)
    {
        var bps = blueprints.ToArray();
        var specWithIndexes = bps.Select(bp => {
            for (int i = 0; i < bp.Specs.Count; i++)
            {
                var spec = bp.Specs[i];
                if (spec is FactionSpec fs)
                {
                    return (i, fs);
                }
            }

            throw new InvalidOperationException(); // Can't happen
        }).ToArray();

        var materialIds = specWithIndexes
            .SelectMany(t => t.fs.MaterialCollectionIds)
            .Distinct()
            .ToImmutableArray();

        for (int i = 0; i < bps.Length; i++)
        {
            var (index, spec) = specWithIndexes[i];
            var bp = bps[i];

            bp.Specs[index] = spec with
            {
                MaterialCollectionIds = materialIds,
                TemplateCollectionIds = [.. spec.TemplateCollectionIds, ConfigurableFactionUtils.ModCollectionId],
                NeedCollectionIds = [.. spec.NeedCollectionIds, ConfigurableFactionUtils.ModCollectionId],
                GoodCollectionIds = [.. spec.GoodCollectionIds, ConfigurableFactionUtils.ModCollectionId],
            };

            yield return bp;
        }
    }

}
