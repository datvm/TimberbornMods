namespace ConfigurableTopBar.Services;

public class GoodSpecModifier(TopBarConfigProvider provider) : BaseSpecTransformer<GoodSpec>
{
    FrozenDictionary<string, CompiledGoodSpecItem>? compiledGoodSpecs;


    public override GoodSpec? Transform(GoodSpec spec)
    {
        compiledGoodSpecs ??= provider.CompileGoodSpecs();
        var g = compiledGoodSpecs[spec.Id];

        return spec with
        {
            GoodGroupId = g.GroupId,
            GoodOrder = g.Order,
        };
    }

}
