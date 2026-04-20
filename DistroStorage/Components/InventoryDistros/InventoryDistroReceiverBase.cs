namespace DistroStorage.Components.InventoryDistros;

public abstract class InventoryDistroReceiverBase(DistroService service) : InventoryDistroComponentBase(service), IDistroReceiver
{

    public override IEnumerable<GoodAmount> Goods
    {
        get
        {
            var inv = Inventory;

            foreach (var g in inv.InputGoods)
            {
                var remainingAmount = inv.UnreservedCapacity(g);

                if (remainingAmount > 0)
                {
                    yield return new(g, remainingAmount);
                }
            }
        }
    }
    public override IEnumerable<string> GoodIds => Goods.Select(g => g.GoodId);

    public Priority Priority { get; protected set; }

    public void TransferIn(GoodAmount good) => Inventory.Give(good);
    public void SetPriority(Priority priority) => Priority = priority;

    public string? CanReceiveGood(HashSet<string> goodIds)
    {
        var inv = Inventory;
        foreach (var g in goodIds)
        {
            if (inv.HasUnreservedCapacity(g))
            {
                return g;
            }
        }

        return null;
    }
}
