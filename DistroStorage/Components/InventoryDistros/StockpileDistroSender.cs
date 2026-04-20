namespace DistroStorage.Components.InventoryDistros;

[AddTemplateModule2(typeof(Stockpile))]
public class StockpileDistroSender(DistroService service) : InventoryDistroSenderBase(service), IDuplicable<StockpileDistroSender>
{
    static readonly ComponentKey StaticSaveKey = new(nameof(StockpileDistroSender));

#nullable disable
    Stockpile stockpile;
#nullable enable

    public override Inventory Inventory => stockpile.Inventory;
    public override bool EnabledByDefault => service.StockpileEnabledDefault;

    protected override ComponentKey SaveKey => StaticSaveKey;

    public override void Awake()
    {
        base.Awake();
        stockpile = GetComponent<Stockpile>();
    }

    public void DuplicateFrom(StockpileDistroSender source) => Deserialize(source.Serialize());
}
