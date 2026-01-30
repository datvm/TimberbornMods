namespace ConfigurableFaction.Services.SpecAppenders;

[MultiBind(typeof(ISpecModifier))]
public class FactionSpecModifier : BaseSpecModifier<FactionSpec>
{

    protected override IEnumerable<NamedSpec<FactionSpec>> Modify(IEnumerable<NamedSpec<FactionSpec>> specs)
    {
        var list = specs.ToArray();
        var materialIds = list
            .SelectMany(f => f.Spec.MaterialCollectionIds)
            .Distinct()
            .ToImmutableArray();

        foreach (var spec in list)
        {
            var original = spec.Spec;

            NamedSpec<FactionSpec> namedSpec = new(spec.Name, original with
            {
                MaterialCollectionIds = materialIds,
                TemplateCollectionIds = [.. original.TemplateCollectionIds, ConfigurableFactionUtils.ModCollectionId],
                NeedCollectionIds = [.. original.NeedCollectionIds, ConfigurableFactionUtils.ModCollectionId],
                GoodCollectionIds = [.. original.GoodCollectionIds, ConfigurableFactionUtils.ModCollectionId],
            });
            
            yield return namedSpec;
        }
    }

}
