namespace ThePostOffice.Components;

[AddTemplateModule2(typeof(PostOfficeLocalInventorySpec))]
public class PostOfficeInventory : BaseComponent, IAwakableComponent, IFinishedStateListener, IGoodDisallower
{
    readonly Dictionary<string, int> limits = [];

#nullable disable
    PostOfficeSender sender;
    PostOfficeReceiver receiver;
    public Inventory Inventory { get; private set; }
#nullable enable

    public event EventHandler<DisallowedGoodsChangedEventArgs>? DisallowedGoodsChanged;

    public void Awake()
    {
        sender = GetComponent<PostOfficeSender>();
        receiver = GetComponent<PostOfficeReceiver>();
    }

    public void InitializeInventory(Inventory inventory)
    {
        Inventory = inventory;
        inventory.InventoryStockChanged += OnInventoryStockChanged;
    }

    void OnInventoryStockChanged(object sender, InventoryStockChangedEventArgs args)
    {
        if (!receiver)
        {
            return;
        }

        DisallowedGoodsChanged?.Invoke(this, new(args.GoodAmount.GoodId));
    }

    public void OnEnterFinishedState()
    {
        if (Inventory)
        {
            Inventory.Enable();
        }

        if (sender)
        {
            sender.NotifyFinished();
        }
        else if (receiver)
        {
            receiver.NotifyFinished();
        }
    }

    public void OnExitFinishedState()
    {
        if (Inventory)
        {
            Inventory.Disable();
        }

        if (sender)
        {
            sender.OnBuildingUnfinished();
        }
        else if (receiver)
        {
            receiver.OnBuildingUnfinished();
        }
    }

    public int AllowedAmount(string goodId) => limits.GetValueOrDefault(goodId);

    public void NotifyLimitsChanged()
    {
        Dictionary<string, int> next = [];
        if (sender)
        {
            sender.CopyAccepted(next);
        }
        else if (receiver)
        {
            receiver.CopyAccepted(next);
        }

        HashSet<string> ids = [..limits.Keys];
        foreach (var id in next.Keys)
        {
            ids.Add(id);
        }

        foreach (var id in ids)
        {
            var amount = next.GetValueOrDefault(id);
            if (limits.GetValueOrDefault(id) == amount)
            {
                continue;
            }

            if (amount == 0)
            {
                limits.Remove(id);
            }
            else
            {
                limits[id] = amount;
            }

            DisallowedGoodsChanged?.Invoke(this, new(id));
        }
    }
}
