namespace ModdableTimberborn.UpdatableEntityStats.Implementations;

public class StockpileIconStat(IGoodService goodService) : ComponentUpdatableEntityStatBase<Sprite?, Stockpile>, IImageStat
{
    public override string Id => "StockpileIcon";

    protected override IEntityStatTracker<Sprite?>? GetComponentTracker(UpdatableEntityStatComponent statComp, Stockpile comp) 
        => comp.Inventory._goodDisallower is SingleGoodAllower allower
            ? new StockpileIconStatTracker(allower, statComp, goodService)
            : null;

}

public class StockpileIconStatTracker(
    SingleGoodAllower allower,
    UpdatableEntityStatComponent comp,
    IGoodService goodService) : StatTrackerBase<Sprite?>(comp), IImageStatTracker
{
    public override string ValueFormatted => Value?.name ?? "?";

    protected override Sprite? CalculateValue() 
        => allower.HasAllowedGood ? goodService.GetGood(allower.AllowedGood).Icon.Asset : null;

    protected override void OnPause()
    {
        allower.DisallowedGoodsChanged -= OnGoodChanged;
    }

    protected override void OnStart()
    {
        allower.DisallowedGoodsChanged += OnGoodChanged;
    }

    void OnGoodChanged(object sender, DisallowedGoodsChangedEventArgs e)
        => UpdateValue();
}
