namespace DistroStorage.Components.InventoryDistros;

public abstract class InventoryDistroReceiverBase(DistroService service) : InventoryDistroComponentBase(service), IDistroReceiver
{
    static readonly ComponentKey SaveKey = new(nameof(InventoryDistroReceiverBase));
    static readonly PropertyKey<int> PriorityKey = new("Priority");

    PausableBuilding? pausableBuilding;

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

    public Priority Priority { get; protected set; } = Priority.Normal;

    protected override bool CalculateActive() => base.CalculateActive() && (!pausableBuilding || !pausableBuilding!.Paused);

    public override void Awake()
    {
        pausableBuilding = this.GetComponentOrNull<PausableBuilding>();
        
        if (pausableBuilding)
        {
            pausableBuilding!.PausedChanged += (_, _) => MarkActiveDirty();
        }

        base.Awake();
    }

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

    public override void Save(IEntitySaver entitySaver)
    {
        base.Save(entitySaver);

        var s = entitySaver.GetComponent(SaveKey);
        s.Set(PriorityKey, (int)Priority);
    }

    public override void Load(IEntityLoader entityLoader)
    {
        base.Load(entityLoader);

        if (!entityLoader.TryGetComponent(SaveKey, out var s)) { return; }

        if (s.Has(PriorityKey))
        {
            Priority = (Priority)s.Get(PriorityKey);
        }
    }

    public virtual DistroReceiverSerializableModel Serialize() => new(Enabled, Priority);
    public virtual void Deserialize(DistroReceiverSerializableModel model)
    {
        if (model.Enabled != Enabled)
        {
            SetEnabled(model.Enabled);
        }

        if (model.Priority != Priority)
        {
            SetPriority(model.Priority);
        }
    }

}
