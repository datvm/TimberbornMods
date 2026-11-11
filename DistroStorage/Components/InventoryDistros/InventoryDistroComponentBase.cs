namespace DistroStorage.Components.InventoryDistros;

public abstract class InventoryDistroComponentBase(DistroService service) : DistroComponentBase(service)
{

    public abstract Inventory Inventory { get; }

    int allowedSignature;

    public override void InitializeEntity()
    {
        base.InitializeEntity();
        allowedSignature = AllowedSignature();

        Inventory.InventoryChanged += (_, _) => OnInventoryUpdated();
        Inventory.InventoryStockChanged += (_, _) => OnInventoryUpdated();
        Inventory.InventoryEnabled += (_, _) => MarkActiveDirty();
        Inventory.InventoryDisabled += (_, _) => MarkActiveDirty();
    }

    protected override bool CanLink(DistroComponentBase other)
    {
        if (other is not InventoryDistroComponentBase right) { return true; }
        if (Inventory is not { } mine || right.Inventory is not { } theirs) { return false; }

        var sender = IsSender ? mine : theirs;
        var receiver = IsSender ? theirs : mine;

        foreach (var good in sender.OutputGoods)
        {
            if (sender.LimitedAmount(good) <= 0) { continue; }
            if (receiver.Takes(good) && receiver.LimitedAmount(good) > 0)
            {
                return true;
            }
        }

        return false;
    }

    void OnInventoryUpdated()
    {
        MarkActiveDirty();

        var next = AllowedSignature();
        if (next == allowedSignature) { return; }

        allowedSignature = next;
        RefreshGoodLinks();
    }

    int AllowedSignature()
    {
        if (Inventory is not { } inventory) { return 0; }

        var hash = 17;
        var goods = IsSender ? inventory.OutputGoods : inventory.InputGoods;
        foreach (var good in goods)
        {
            if (inventory.LimitedAmount(good) <= 0) { continue; }
            hash = unchecked(hash * 31 + good.GetHashCode());
        }

        return hash;
    }

    protected override bool CalculateActive() 
        => base.CalculateActive()
        && Inventory is not null
        && Inventory.Enabled;
}
