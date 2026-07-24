namespace StockpileBalancer.Components;

public class BalancerGroup(string goodId, ImmutableHashSet<StockpileBalancerComponent> balancers)
{
    public string GoodId => goodId;
    public ImmutableHashSet<StockpileBalancerComponent> Balancers => balancers;

    public bool Balanced { get; set; }
    public void MarkDirty() => Balanced = false;
}
