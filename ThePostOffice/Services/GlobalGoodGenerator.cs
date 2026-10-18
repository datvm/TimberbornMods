namespace ThePostOffice.Services;

[MultiBind(typeof(ISpecServiceTailRunner))]
public class GlobalGoodGenerator : ISpecServiceTailRunner
{

    public void Run(SpecService specService)
    {
        var cache = specService._cachedBlueprintsBySpecs;
        var globalList = cache[typeof(GlobalGoodSpec)];
        var known = globalList
            .Select(lz => lz.Value.GetSpec<GlobalGoodSpec>().Id)
            .ToHashSet();

        foreach (var gBp in cache[typeof(GoodSpec)].Select(lz => lz.Value))
        {
            var goodSpec = gBp.GetSpec<GoodSpec>();
            var id = goodSpec.Id;
            if (!known.Add(id))
            {
                continue;
            }

            var blueprint = new Blueprint("GlobalGood.PostOffice." + id, [
                new GlobalGoodSpec
                {
                    Id = id,
                    CopiedGoodId = id,
                    Order = goodSpec.GoodOrder,
                }
            ], []);
            globalList.Add(new(blueprint));
        }
    }

}
