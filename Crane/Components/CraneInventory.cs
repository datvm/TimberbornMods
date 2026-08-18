namespace Crane.Components;

[AddTemplateModule2(typeof(CraneComponent))]
public class CraneInventory : BaseComponent, IAwakableComponent, IFinishedStateListener, IDeletableEntity, IGoodDisallower
{
    CraneComponent crane = null!;
    readonly Dictionary<string, int> limits = [];
    readonly HashSet<IMaterialCraneJob> subscribed = [];
    bool dumping;

    public Inventory Inventory { get; private set; } = null!;

    public IReadOnlyDictionary<string, int> Limits => limits;

    public event EventHandler<DisallowedGoodsChangedEventArgs>? DisallowedGoodsChanged;

    public void Awake()
    {
        crane = GetComponent<CraneComponent>();
    }

    public void InitializeInventory(Inventory inventory)
    {
        Inventory = inventory;
        Inventory.InventoryChanged += OnInventoryChanged;
    }

    public void OnEnterFinishedState()
    {
        if (Inventory)
        {
            Inventory.Enable();
        }

        OnJobsChanged();
    }

    public void OnExitFinishedState()
    {
        Flush();
        if (Inventory)
        {
            Inventory.Disable();
        }

        UnsubscribeMaterials();
    }

    public void DeleteEntity()
    {
        if (Inventory)
        {
            Inventory.InventoryChanged -= OnInventoryChanged;
        }

        UnsubscribeMaterials();
    }

    public int AllowedAmount(string goodId) => limits.GetValueOrDefault(goodId);

    public void OnJobsChanged()
    {
        if (!crane)
        {
            crane = GetComponent<CraneComponent>();
        }

        if (!crane)
        {
            return;
        }

        UnsubscribeMaterials();
        foreach (var job in crane.Tower.Jobs)
        {
            if (job is IMaterialCraneJob material)
            {
                material.MaterialsChanged += OnMaterialNeedChanged;
                subscribed.Add(material);
            }
        }

        Flush();
    }

    void OnInventoryChanged(object sender, InventoryChangedEventArgs e) => Flush();

    void OnMaterialNeedChanged(object sender, EventArgs e) => Flush();

    void Flush()
    {
        if (dumping)
        {
            return;
        }

        dumping = true;
        try
        {
            DumpToJobs();
            RefreshLimits();
        }
        finally
        {
            dumping = false;
        }
    }

    void DumpToJobs()
    {
        if (!crane || !Inventory || !Inventory.Enabled || Inventory.IsEmpty)
        {
            return;
        }

        foreach (var job in crane.Tower.Jobs)
        {
            if (job is not IMaterialCraneJob material)
            {
                continue;
            }

            foreach (var need in material.GetMaterials())
            {
                var available = Inventory.UnreservedAmountInStock(need.GoodId);
                var amount = Math.Min(available, need.Amount);
                if (amount <= 0)
                {
                    continue;
                }

                Inventory.TakeExisting(new(need.GoodId, amount));
                var given = material.AddMaterial(new(need.GoodId, amount));
                if (given < amount)
                {
                    Inventory.GiveExistingIgnoringCapacity(new(need.GoodId, amount - given));
                }
            }

            if (Inventory.IsEmpty)
            {
                return;
            }
        }
    }

    void RefreshLimits()
    {
        foreach (var key in limits.Keys.ToArray())
        {
            limits[key] = 0;
        }

        if (crane)
        {
            foreach (var job in crane.Tower.Jobs)
            {
                if (job is not IMaterialCraneJob material)
                {
                    continue;
                }

                foreach (var need in material.GetMaterials())
                {
                    limits[need.GoodId] = limits.GetValueOrDefault(need.GoodId) + need.Amount;
                }
            }
        }

        foreach (var key in limits.Keys)
        {
            DisallowedGoodsChanged?.Invoke(this, new(key));
        }
    }

    void UnsubscribeMaterials()
    {
        foreach (var job in subscribed)
        {
            job.MaterialsChanged -= OnMaterialNeedChanged;
        }

        subscribed.Clear();
    }

}
