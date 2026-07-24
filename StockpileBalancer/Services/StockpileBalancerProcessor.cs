namespace StockpileBalancer.Services;

[BindSingleton]
public class StockpileBalancerProcessor(StockpileBalancerService balancerService) : ITickableSingleton
{
    public void Tick()
    {
        foreach (var group in balancerService.Groups)
        {
            if (group.Balanced) { continue; }

            BalanceGroup(group);
        }
    }


    void BalanceGroup(BalancerGroup group)
    {
        if (group.Balancers.Count <= 1)
        {
            group.Balanced = true;
            return;
        }

        StockpileBalancerComponent? min = null, max = null;
        var minAmount = int.MaxValue;
        var maxAmount = int.MinValue;

        foreach (var balancer in group.Balancers)
        {
            if (balancer.BalancerDisabled) { continue; }

            var amount = balancer.CurrentAmount;
            if (amount < minAmount && balancer.FreeCapacity > 0)
            {
                min = balancer;
                minAmount = amount;
            }

            if (amount > maxAmount)
            {
                max = balancer;
                maxAmount = amount;
            }
        }

        if (min is null || max is null || maxAmount - minAmount <= 1)
        {
            group.Balanced = true;
            return;
        }

        max.RemoveGood(1);
        min.AddGood(1);
    }

}
