namespace BeavlineLogistics.Services;

public class StockpileBalancerService(
    IDayNightCycle dayNightCycle,
    IBlockService blockService
) : ITickableSingleton
{
    int prevDay = -1, prevHour = -1;

    readonly HashSet<BeavlineBalancerComponent> balancers = [];
    public IReadOnlyCollection<BeavlineBalancerComponent> Balancers => balancers;

    public void Register(BeavlineBalancerComponent comp)
    {
        balancers.Add(comp);
    }

    public void Unregister(BeavlineBalancerComponent comp)
    {
        balancers.Remove(comp);
    }

    public void Tick()
    {
        var day = dayNightCycle.DayNumber;
        var hour = (int)dayNightCycle.HoursPassedToday;
        if (day == prevDay && hour == prevHour) { return; }

        prevDay = day;
        prevHour = hour;
        Balance();
    }

    // This one list all connected balancers of the same good, regardless of imbalance
    public IReadOnlyList<BeavlineBalancerComponent> FindCluster(BeavlineBalancerComponent start)
    {
        if (!balancers.Contains(start)) { return []; }

        List<BeavlineBalancerComponent> result = [start];

        var g = start.GoodId;
        if (g is null) { return result; }

        var targets = balancers.ToHashSet();
        targets.Remove(start);
        Stack<BeavlineBalancerComponent> stack = new(result);

        while (stack.Count > 0)
        {
            var curr = stack.Pop();
            var adjs = blockService.FindAdjacentBlockObjects(curr.BlockObject);
            foreach (var adj in adjs)
            {
                var balancer = adj.GetComponentFast<BeavlineBalancerComponent>();
                if (!balancer || !targets.Contains(balancer) || balancer.GoodId != g) { continue; }

                targets.Remove(balancer);
                stack.Push(balancer);
                result.Add(balancer);
            }
        }

        return result;
    }

    void Balance()
    {
        if (balancers.Count == 0) { return; }

        var targets = balancers.ToHashSet();
        while (targets.Count > 0)
        {
            var target = targets.First();
            var cluster = FindImbalancedCluster(target, targets);
            if (cluster.Count <= 1) { continue; }

            var g = target.GoodId;
            var currs = cluster.Select(q => q.MovableAmount).ToArray();
            var total = currs.Sum();
            var capacities = cluster.Select(q => q.StorableAmount).ToArray();

            var distribution = DistributeEvenCapped(total, capacities);
            if (distribution.SequenceEqual(currs)) { continue; } // This is the false flag case mentioned in FindImbalancedCluster

            for (int i = 0; i < cluster.Count; i++)
            {
                var node = cluster[i];
                var shouldHave = distribution[i];
                var curr = currs[i];

                if (curr > shouldHave)
                {
                    node.Inventory.Take(new(g, curr - shouldHave));
                }
                else if (curr < shouldHave)
                {
                    node.Inventory.Give(new(g, shouldHave - curr));
                }
            }
        }
    }

    /// <summary>
    /// Distribute `total` into `m` parts as evenly as possible, with each part capped by `cap[i]`.
    /// </summary>
    static int[] DistributeEvenCapped(int total, IReadOnlyList<int> cap)
    {
        var m = cap.Count;
        if (m == 0 || total <= 0) { return new int[m]; }

        // Compute total capacity and max cap
        var sumCap = 0; 
        var hi = 0;
        for (var i = 0; i < m; i++)
        {
            sumCap += cap[i];
            if (cap[i] > hi)
            {
                hi = cap[i];
            }
        }
        if (total > sumCap) { throw new ArgumentOutOfRangeException(nameof(total)); }

        // Binary search for the largest even fill level (L) that still fits total
        var lo = 0;
        while (lo < hi)
        {
            var mid = (lo + hi + 1) >> 1;
            var s = 0;
            for (var i = 0; i < m; i++)
            {
                s += Math.Min(cap[i], mid);
                if (s > total)
                {
                    break;
                }
            }
            if (s <= total)
            {
                lo = mid;
            }
            else
            {
                hi = mid - 1;
            }
        }
        var L = lo; // final even fill level

        // First pass: assign each up to min(cap, L)
        var res = new int[m];
        var used = 0;
        for (var i = 0; i < m; i++)
        {
            var c = res[i] = Math.Min(cap[i], L);
            used += c;
        }

        // Second pass: distribute remaining one by one
        var r = total - used;
        for (var i = 0; i < m && r > 0; i++)
        {
            if (cap[i] > L)
            {
                res[i]++; 
                r--;
            }
        }

        return res;
    }

    IReadOnlyList<BeavlineBalancerComponent> FindImbalancedCluster(BeavlineBalancerComponent start, HashSet<BeavlineBalancerComponent> targets)
    {
        targets.Remove(start);

        List<BeavlineBalancerComponent> result = [start];
        var g = start.GoodId;
        if (g is null) { return result; }

        var amount = start.MovableAmount;
        int min = amount, max = amount;
        Stack<BeavlineBalancerComponent> stack = new(result);
        while (stack.Count > 0)
        {
            var curr = stack.Pop();
            var adjs = blockService.FindAdjacentBlockObjects(curr.BlockObject);

            foreach (var adj in adjs)
            {
                var balancer = adj.GetComponentFast<BeavlineBalancerComponent>();
                if (!balancer || !targets.Contains(balancer) || balancer.GoodId != g) { continue; }

                targets.Remove(balancer);
                stack.Push(balancer);
                result.Add(balancer);

                var adjAmount = balancer.MovableAmount;
                if (adjAmount < min) { min = adjAmount; }
                if (adjAmount > max) { max = adjAmount; }
            }
        }

        // Note: this may be a false flag due to cap. Nevertheles, it's cheaper to just run the distribution algo in that case
        // Example: items [1, 50, 50] with cap [1, 100, 100] is already balanced but will be marked as imbalanced
        var isImbalanced = max - min > 1;
        return isImbalanced ? result : [];
    }

}
