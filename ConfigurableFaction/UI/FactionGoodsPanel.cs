namespace ConfigurableFaction.UI;

public class FactionGoodsPanel(FactionOptionsService optionsService, ILoc t) : FactionIdsPanel<GoodSpec>(optionsService, t)
{
    protected override string HeaderLoc { get; } = "LV.CFac.Goods";
    protected override HashSet<string> OptionsList => options.Goods;
    protected override HashSet<string> LockedInList => options.LockedInGoods;
    protected override HashSet<string> ExistingList => options.ExistingGoods;

    protected override string GetId(GoodSpec spec) => spec.Id;
    protected override ImmutableArray<GoodSpec> GetSpecs(FactionInfo faction) => faction.Goods;
    protected override string GetText(GoodSpec spec) => spec.DisplayName.Value;

    protected override void OnRowChanged(string id, bool add)
    {
        if (add)
        {
            optionsService.AddGood(options, id);
        }
        else
        {
            optionsService.RemoveGood(options, id);
        }
    }

}
