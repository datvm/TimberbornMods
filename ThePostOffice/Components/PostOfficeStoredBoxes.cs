namespace ThePostOffice.Components;

public class PostOfficeStoredBoxes(PostOfficeBoxVisuals visuals, PostOfficeService offices) : BaseComponent, IAwakableComponent, IInitializableEntity, IDeletableEntity, IFinishedStateListener
{
    PostOfficeInventory inventory = null!;
    PostOfficeReceiver? receiver;
    BuildingModel model = null!;
    BlockObject block = null!;
    PostOfficeBoxPile boxes = null!;
    bool listening;
    bool ready;

    public void Awake()
    {
        inventory = GetComponent<PostOfficeInventory>();
        receiver = GetComponent<PostOfficeReceiver>();
        model = GetComponent<BuildingModel>();
        block = GetComponent<BlockObject>();
        boxes = new();
    }

    public void InitializeEntity() => Listen();

    public void DeleteEntity()
    {
        var stock = inventory.Inventory;
        if (listening && stock)
        {
            stock.InventoryStockChanged -= OnStock;
        }

        boxes.Clear();
    }

    public void OnEnterFinishedState()
    {
        ready = true;
        Listen();
        Refresh();
    }

    public void OnExitFinishedState() => boxes.Clear();

    void Listen()
    {
        if (listening)
        {
            return;
        }

        var stock = inventory.Inventory;
        if (!stock)
        {
            return;
        }

        stock.InventoryStockChanged += OnStock;
        listening = true;
    }

    void OnStock(object sender, InventoryStockChangedEventArgs args) => Refresh();

    void Refresh()
    {
        var finished = model.FinishedModel;
        var stock = inventory.Inventory;
        if (!ready || !block.IsFinished || !finished || !stock)
        {
            boxes.Clear();
            return;
        }

        var capacity = receiver is not null ? receiver.InventoryCapacity : offices.ParcelWeight;
        visuals.Show(
            boxes,
            finished.transform,
            visuals.OfficeShares(stock, capacity),
            PostOfficeBoxPlan.OfficeSlots,
            PostOfficeBoxPlan.OfficeLayers);
    }
}
