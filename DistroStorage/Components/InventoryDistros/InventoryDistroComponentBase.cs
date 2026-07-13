namespace DistroStorage.Components.InventoryDistros;

public abstract class InventoryDistroComponentBase(DistroService service) : DistroComponentBase(service)
{

    public abstract Inventory Inventory { get; }

    public override void InitializeEntity()
    {
        base.InitializeEntity();

        Inventory.InventoryChanged += (_, _) => MarkActiveDirty();
        Inventory.InventoryStockChanged += (_, _) => MarkActiveDirty();
        Inventory.InventoryEnabled += (_, _) => MarkActiveDirty();
        Inventory.InventoryDisabled += (_, _) => MarkActiveDirty();
    }

    protected override bool CalculateActive() 
        => base.CalculateActive()
        && Inventory is not null
        && Inventory.Enabled;
}
