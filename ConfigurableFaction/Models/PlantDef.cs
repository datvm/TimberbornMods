namespace ConfigurableFaction.Models;

public class PlantDef(PlantableSpec Plantable, Blueprint Blueprint, string path, DataAggregatorService dataAggregator, ILoc t) : TemplateDefBase(Blueprint, path, dataAggregator, t)
{
    public PlantableSpec Plantable { get; } = Plantable;
    public string PlanterId => Plantable.ResourceGroup;
    public override string PlanterGroup => Plantable.ResourceGroup;

    public NaturalResourceSpec NaturalResourceSpec { get; } = Blueprint.GetSpec<NaturalResourceSpec>();
    public override int Order => NaturalResourceSpec.Order;

    protected override void InitializeRequirements(DataAggregatorService dataAggregator)
    {
        RequiredGoods = [
            ..CheckForComponent<CuttableSpec>(dataAggregator, c => [c.Yielder.Yield.Id]),
            ..CheckForComponent<GatherableSpec>(dataAggregator, c => [c.Yielder.Yield.Id]),
        ];
    }

    IEnumerable<GoodDef> CheckForComponent<T>(DataAggregatorService dataAggregator, Func<T, IEnumerable<string>> goodsFunc)
        where T : ComponentSpec
    {
        var comp = Blueprint.GetSpec<T>();
        if (comp is null) { yield break; }

        foreach (var goodId in goodsFunc(comp))
        {
            yield return dataAggregator.Goods.ItemsByIds[goodId];
        }
    }
}
