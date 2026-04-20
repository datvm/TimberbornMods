namespace DistroStorage.Components.InventoryDistros;

public abstract class InventoryDistroSenderBase(DistroService service) : InventoryDistroComponentBase(service), IDistroSender
{
    protected abstract ComponentKey SaveKey { get; }

    public override IEnumerable<GoodAmount> Goods => Inventory.UnreservedTakeableStock();
    public override IEnumerable<string> GoodIds => Goods.Select(g => g.GoodId);

    public IDistroSenderTimer Timer { get; } = new DefaultDistroSenderTimer(service.TransferTime);

    public IEnumerable<IDistroReceiver> GetPrioritizedReceivers() => Connections
        .Where(c => c.ActiveAndEnabled)
        .Cast<IDistroReceiver>()
        .OrderByDescending(r => r.Priority);

    public void TransferOut(GoodAmount good) => Inventory.Take(good);

    public override void Save(IEntitySaver entitySaver)
    {
        base.Save(entitySaver);

        var s = entitySaver.GetComponent(SaveKey);
        Timer.Save(s);
    }

    public override void Load(IEntityLoader entityLoader)
    {
        base.Load(entityLoader);

        if (entityLoader.TryGetComponent(SaveKey, out var s))
        {
            Timer.Load(s);
        }
    }

    public virtual DistroSenderSerializableModel Serialize() => new(Enabled);
    public virtual void Deserialize(DistroSenderSerializableModel model)
    {
        if (model.Enabled != Enabled)
        {
            SetEnabled(model.Enabled);
        }
    }
}
