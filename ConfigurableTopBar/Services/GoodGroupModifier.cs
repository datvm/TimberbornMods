namespace ConfigurableTopBar.Services;

public class GoodGroupModifier(TopBarConfigProvider provider) : BaseSpecModifier<GoodGroupSpec>
{

    protected override IEnumerable<NamedSpec<GoodGroupSpec>> Modify(IEnumerable<NamedSpec<GoodGroupSpec>> specs)
    {
        foreach (var spec in provider.CompileGoodGroupSpecs())
        {
            yield return new(nameof(GoodGroupSpec) + "." + spec.Id, spec);
        }
    }

}