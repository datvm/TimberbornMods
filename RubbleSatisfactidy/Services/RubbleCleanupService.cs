namespace RubbleSatisfactidy.Services;

public class RubbleCleanupService(
    RecoveredGoodStackFragment fragment,
    ILoc t,
    EntityRegistry entityRegistry,
    DialogBoxShower diag,
    IGoodService goodService,
#pragma warning disable CS9113 // Just for DI order
    IEntityPanel _
#pragma warning restore CS9113 // Parameter is unread.
) : ILoadableSingleton
{

    public void Load()
    {
        var parent = fragment._root.Q("RecoveredGoodStackFragment");

        var btn = parent.AddGameButton(t.T("LV.RSa.CleanUpAll"), onClick: CleanUpAll, stretched: true)
            .SetPadding(paddingY: 5)
            .SetMarginBottom(5);

        parent.Insert(0, btn);
    }

    void CleanUpAll()
    {
        var stacks = GatherRecoveredGoodStacks().ToArray();
        var goods = GetStacksGoods(stacks);

        var stockpiles = GatherStockpiles([.. goods.Select(q => q.Key)]);
        TryToDistributeGoods(stacks, stockpiles);
    }

    void TryToDistributeGoods(RecoveredGoodStack[] stacks, Dictionary<string, List<Stockpile>> stockpiles)
    {
        Dictionary<string, int> uncapacity = [];

        foreach (var stack in stacks)
        {
            var inv = stack.Inventory;
            var goods = inv.UnreservedStock().ToArray();

            foreach (var good in goods)
            {
                var id = good.GoodId;
                var amount = good.Amount;

                if (amount == 0) { continue; }

                var goodStockpiles = stockpiles[id];
                foreach (var sp in goodStockpiles)
                {
                    var capacity = sp.Inventory.UnreservedCapacity(id);
                    if (capacity == 0) { continue; }

                    var toMove = Math.Min(amount, capacity);
                    var gAmount = new GoodAmount(id, toMove);
                    sp.Inventory.Give(gAmount);
                    inv.Take(gAmount);
                    amount -= toMove;
                }

                if (amount > 0)
                {
                    uncapacity[id] = uncapacity.TryGetValue(id, out var count) ? count + amount : amount;
                }
            }
        }

        if (uncapacity.Count >0)
        {
            ShowUncapacityMessage(uncapacity);
        }
    }

    void ShowUncapacityMessage(Dictionary<string, int> uncapacity)
    {
        var msg = t.T("LV.RSa.NotEnoughSpace", string.Join(
            Environment.NewLine,
            uncapacity.Select(kv => t.T("LV.RSa.Item", kv.Value, goodService.GetGood(kv.Key).PluralDisplayName.Value))
        ));

        diag.Create()
            .SetMessage(msg)
            .Show();
    }

    IEnumerable<RecoveredGoodStack> GatherRecoveredGoodStacks()
    {
        foreach (var e in entityRegistry.Entities)
        {
            var gs = e.GetComponentFast<RecoveredGoodStack>();
            if (gs && gs.Inventory.HasAnyUnreservedStock)
            {
                yield return gs;
            }
        }
    }

    Dictionary<string, int> GetStacksGoods(IEnumerable<RecoveredGoodStack> stacks)
    {
        Dictionary<string, int> dict = [];

        foreach (var s in stacks)
        {
            var goods = s.Inventory.UnreservedStock();

            foreach (var g in goods)
            {
                dict[g.GoodId] = dict.TryGetValue(g.GoodId, out var count) ? count + g.Amount : g.Amount;
            }
        }

        return dict;
    }

    Dictionary<string, List<Stockpile>> GatherStockpiles(HashSet<string> goods)
    {
        Dictionary<string, List<Stockpile>> dict = [];

        foreach (var e in entityRegistry.Entities)
        {
            var bo = e.GetComponentFast<BlockObject>();
            if (!bo || !bo.IsFinished) { continue; }

            var stockpile = e.GetComponentFast<Stockpile>();
            if (!stockpile) { continue; }

            var inv = stockpile.Inventory;
            foreach (var g in goods)
            {
                if (inv.UnreservedCapacity(g) > 0)
                {
                    if (!dict.TryGetValue(g, out var list))
                    {
                        list = dict[g] = [];
                    }
                    list.Add(stockpile);

                    break;
                }
            }
        }

        foreach (var g in goods)
        {
            if (!dict.ContainsKey(g))
            {
                dict[g] = [];
            }
        }

        return dict;
    }

}
