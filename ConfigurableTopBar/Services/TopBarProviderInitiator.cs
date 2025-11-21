namespace ConfigurableTopBar.Services;

public class TopBarProviderInitiator(
    TopBarConfigProvider provider,
    GoodGroupModifier goodGroupSpecModifier,
    GoodSpecModifier goodSpecModifier
) : ISpecServiceTailRunner
{

    public void Run(SpecService specService)
    {
        provider.Initialize(specService);

        Replace<GoodGroupSpec>(specService, goodGroupSpecModifier);
        Replace<GoodSpec>(specService, goodSpecModifier);
    }

    void Replace<T>(SpecService specService, ISpecModifier modifier) where T : ComponentSpec
    {
        var bps = specService._cachedBlueprints[typeof(T)]
            .Select(q => new EditableBlueprint(q.Value));

        var modified = modifier.Modify(bps);

        specService._cachedBlueprints[typeof(T)] = [.. modified
            .Select(q => {
                var bp = q.ToBlueprint();
                return new Lazy<Blueprint>(bp);
            })];
    }

}
