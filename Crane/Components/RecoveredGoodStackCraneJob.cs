namespace Crane.Components;

[AddTemplateModule2(typeof(RecoveredGoodStack))]
public class RecoveredGoodStackCraneJob(
    RecoveredGoodStackCraneJobProvider provider
) : BaseComponent, IInitializableEntity, IDeletableEntity, ICraneJob
{
    public const float ItemsPerHour = 60f;

    RecoveredGoodStack stack = null!;
    BlockObject bo = null!;
    BuilderPrioritizable? prioritizable;
    BoundsInt workableArea;
    bool listed;
    float leftover;
    int initialItems;

    public Priority Priority => prioritizable?.Priority ?? Priority.Normal;

    public bool IsAvailable => stack && stack.Inventory && HasUnreservedStock();

    public float Progress
    {
        get
        {
            if (!stack || !stack.Inventory)
            {
                return 1f;
            }

            var remaining = stack.Inventory.TotalAmountInStock;
            if (remaining > initialItems)
            {
                initialItems = remaining;
            }

            if (initialItems <= 0)
            {
                return 1f;
            }

            return 1f - remaining / (float)initialItems;
        }
    }

    public event EventHandler? AvailabilityChanged;
    public event EventHandler<PriorityChangedEventArgs>? PriorityChanged;

    public string JobNameLoc => "LV.Cr.JobGather";

    public void InitializeEntity()
    {
        stack = GetComponent<RecoveredGoodStack>();
        bo = GetComponent<BlockObject>();
        prioritizable = GetComponent<BuilderPrioritizable>();
        workableArea = bo.GetConstructionBounds();

        if (prioritizable)
        {
            prioritizable!.PriorityChanged += OnPriorityChanged;
        }

        if (stack.Inventory)
        {
            stack.Inventory.InventoryChanged += OnInventoryChanged;
            initialItems = stack.Inventory.TotalAmountInStock;
        }

        ListJob();
    }

    public void DeleteEntity()
    {
        if (stack && stack.Inventory)
        {
            stack.Inventory.InventoryChanged -= OnInventoryChanged;
        }

        if (prioritizable)
        {
            prioritizable!.PriorityChanged -= OnPriorityChanged;
        }

        Unlist();
    }

    public bool IsForCrane(CraneComponent crane) => crane.Tower.WorkingBounds.Overlaps(workableArea);

    public void ProgressJob(CraneComponent crane, float hours)
    {
        if (!IsAvailable)
        {
            return;
        }

        leftover += hours * ItemsPerHour;
        var items = (int)leftover;
        leftover -= items;
        if (items <= 0)
        {
            return;
        }

        if (crane.TryProcessRubble(stack, items))
        {
            return;
        }

        var dest = crane.GetComponent<CraneInventory>();
        if (!dest || !dest.Inventory || !dest.Inventory.Enabled)
        {
            return;
        }

        foreach (var good in stack.Inventory.UnreservedStock().ToArray())
        {
            var available = stack.Inventory.UnreservedAmountInStock(good.GoodId);
            var amount = Math.Min(Math.Min(good.Amount, items), available);
            if (amount <= 0)
            {
                continue;
            }

            var moved = new GoodAmount(good.GoodId, amount);
            stack.Inventory.TakeExisting(moved);
            dest.Inventory.GiveExistingIgnoringCapacity(moved);
            items -= amount;
            if (items <= 0)
            {
                return;
            }
        }
    }

    public void SetPriority(Priority priority)
    {
        prioritizable?.SetPriority(priority);
    }

    bool HasUnreservedStock()
    {
        foreach (var good in stack.Inventory.UnreservedStock())
        {
            if (good.Amount > 0)
            {
                return true;
            }
        }

        return false;
    }

    void OnInventoryChanged(object sender, InventoryChangedEventArgs e) => AvailabilityChanged?.Invoke(this, EventArgs.Empty);

    void OnPriorityChanged(object sender, PriorityChangedEventArgs e) => PriorityChanged?.Invoke(this, e);

    void ListJob()
    {
        if (listed)
        {
            return;
        }

        listed = true;
        provider.NotifyNew(this);
    }

    void Unlist()
    {
        if (!listed)
        {
            return;
        }

        listed = false;
        provider.NotifyRemoved(this);
    }

}
