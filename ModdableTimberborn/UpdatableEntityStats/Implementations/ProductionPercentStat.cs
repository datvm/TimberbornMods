namespace ModdableTimberborn.UpdatableEntityStats.Implementations;

public class ProductionPercentStat : ComponentUpdatableEntityPercentStatBase<Manufactory>
{
    public override string Id => "ProductionPercent";

    protected override IEntityPercentStatTracker? GetComponentPercentTracker(UpdatableEntityStatComponent statComp, Manufactory comp)
        => new ProductionPercentStatTracker(comp, statComp);

}

public class ProductionPercentStatTracker(Manufactory manufactory, UpdatableEntityStatComponent comp) : StatPercentTrackerBase(comp), IEntityPercentStatTracker
{
    protected override float CalculateValue() => manufactory.ProductionProgress;
    
    protected override void OnPause()
    {
        manufactory.ProductionProgressed -= OnProgressed;
    }

    protected override void OnStart()
    {
        manufactory.ProductionProgressed += OnProgressed;
    }

    void OnProgressed(object sender, ProductionProgressedEventArgs e) => UpdateValue();
}