namespace BeavlineLogistics.Components;

public class BeavlineBalancerComponent : BaseComponent, IActivableRenovationComponent, IDeletableEntity, IPersistentEntity
{
    static readonly ComponentKey SaveKey = new(nameof(BeavlineBalancerComponent));
    static readonly PropertyKey<bool> DisabledKey = new("Disabled");

    public bool RenovationActive { get; private set; }
    public Action<BuildingRenovation>? ActiveHandler { get; set; }

    public bool Disabled { get; set; }

#nullable disable
    StockpileBalancerService balancerService;
    public Inventory Inventory { get; private set; }
    public SingleGoodAllower SingleGoodAllower { get; private set; }
    public BlockObject BlockObject { get; private set; }
#nullable enable

    public string? GoodId => SingleGoodAllower.HasAllowedGood ? SingleGoodAllower.AllowedGood : null;
    public int MovableAmount => Inventory.UnreservedAmountInStock(GoodId ?? throw new InvalidOperationException());
    public int StorableAmount => Inventory.UnreservedCapacity(GoodId) + MovableAmount;

    [Inject]
    public void Inject(StockpileBalancerService balancerService)
    {
        this.balancerService = balancerService;
    }

    public void Start()
    {
        Inventory = GetComponentFast<Stockpile>().Inventory;
        SingleGoodAllower = GetComponentFast<SingleGoodAllower>();
        BlockObject = GetComponentFast<BlockObject>();

        this.ActivateIfAvailable(BeavlineBalancerRenovationProvider.RenoId);
    }

    public void Activate()
    {
        RenovationActive = true;
        balancerService.Register(this);
    }

    public void DeleteEntity()
    {
        balancerService.Unregister(this);
    }

    public void Save(IEntitySaver entitySaver)
    {
        if (!Disabled) { return; }

        var s = entitySaver.GetComponent(SaveKey);
        s.Set(DisabledKey, Disabled);
    }

    public void Load(IEntityLoader entityLoader)
    {
        if (!entityLoader.TryGetComponent(SaveKey, out var s)) { return; }

        if (s.Has(DisabledKey))
        {
            Disabled = s.Get(DisabledKey);
        }
    }
}
