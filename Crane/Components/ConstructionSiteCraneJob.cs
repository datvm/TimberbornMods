namespace Crane.Components;

[AddTemplateModule2(typeof(ConstructionSite))]
public class ConstructionSiteCraneJob(
    ConstructionCraneJobProvider provider,
    CraneStructureService structureService
) : BaseComponent, IInitializableEntity, IUnfinishedStateListener, IDeletableEntity, IMaterialCraneJob
{
    public ConstructionSite ConstructionSite { get; private set; } = null!;
    BoundsInt workableArea;
    BlockObject bo = null!;
    BlockableObject blockable = null!;
    BuilderPrioritizable? prioritizable;
    readonly List<IConstructionSiteValidator> validators = [];
    bool listed;

    public Priority Priority => prioritizable?.Priority ?? Priority.Normal;

    public bool IsAvailable => ConstructionSite && ConstructionSite.IsOn;

    public event EventHandler? AvailabilityChanged;

    public event EventHandler<PriorityChangedEventArgs>? PriorityChanged;

    public event EventHandler? MaterialsChanged;

    public void InitializeEntity()
    {
        ConstructionSite = GetComponent<ConstructionSite>();
        bo = GetComponent<BlockObject>();
        blockable = GetComponent<BlockableObject>();
        prioritizable = GetComponent<BuilderPrioritizable>();
        GetComponents(validators);
        workableArea = bo.GetConstructionBounds();

        if (prioritizable)
        {
            prioritizable!.PriorityChanged += OnPriorityChanged;
        }
    }

    public void OnEnterUnfinishedState()
    {
        if (ConstructionSite.Inventory)
        {
            ConstructionSite.Inventory.InventoryChanged += OnInventoryChanged;
        }

        blockable.ObjectBlocked += OnAvailabilitySignal;
        blockable.ObjectUnblocked += OnAvailabilitySignal;
        foreach (var validator in validators)
        {
            validator.ValidationStateChanged += OnAvailabilitySignal;
        }

        listed = true;
        provider.NotifyNew(this);
    }

    public void OnExitUnfinishedState() => Unlist();

    public void DeleteEntity()
    {
        if (prioritizable)
        {
            prioritizable!.PriorityChanged -= OnPriorityChanged;
        }

        Unlist();
    }

    public bool IsForCrane(CraneComponent crane)
    {
        if (!bo || !bo.IsUnfinished || GetComponent<CraneComponent>() == crane)
        {
            return false;
        }

        if (GetComponent<CraneSectionComponent>() is { } section && section)
        {
            var owner = section.Crane ?? structureService.FindCraneOfSection(section);
            return owner == crane;
        }

        return crane.Tower.WorkingBounds.Overlaps(workableArea);
    }

    public bool IsMastSection => GetComponent<CraneSectionComponent>();

    public IEnumerable<GoodAmount> GetMaterials()
    {
        if (!ConstructionSite || !ConstructionSite.Inventory)
        {
            yield break;
        }

        foreach (var allowed in ConstructionSite.Inventory.AllowedGoods)
        {
            var goodId = allowed.StorableGood.GoodId;
            var remaining = ConstructionSite.Inventory.UnreservedCapacity(goodId);
            if (remaining > 0)
            {
                yield return new GoodAmount(goodId, remaining);
            }
        }
    }

    public int AddMaterial(GoodAmount material)
    {
        if (!ConstructionSite || !ConstructionSite.Inventory)
        {
            return 0;
        }

        var inventory = ConstructionSite.Inventory;
        var amount = Math.Min(material.Amount, inventory.UnreservedCapacity(material.GoodId));
        if (amount <= 0)
        {
            return 0;
        }

        inventory.GiveExisting(new(material.GoodId, amount));
        return amount;
    }

    public bool CanHammer => ConstructionSite && ConstructionSite.IsOn && ConstructionSite.HasMaterialsToResumeBuilding;

    public void ProgressJob(CraneComponent crane, float hours)
    {
        if (CanHammer)
        {
            ConstructionSite.IncreaseBuildTime(hours);
        }
    }

    public void SetPriority(Priority priority)
    {
        prioritizable?.SetPriority(priority);
    }

    void Unlist()
    {
        if (!listed)
        {
            return;
        }

        listed = false;
        if (ConstructionSite && ConstructionSite.Inventory)
        {
            ConstructionSite.Inventory.InventoryChanged -= OnInventoryChanged;
        }

        if (blockable)
        {
            blockable.ObjectBlocked -= OnAvailabilitySignal;
            blockable.ObjectUnblocked -= OnAvailabilitySignal;
        }

        foreach (var validator in validators)
        {
            validator.ValidationStateChanged -= OnAvailabilitySignal;
        }

        provider.NotifyRemoved(this);
    }

    void OnInventoryChanged(object sender, InventoryChangedEventArgs e)
    {
        MaterialsChanged?.Invoke(this, EventArgs.Empty);
        AvailabilityChanged?.Invoke(this, EventArgs.Empty);
    }

    void OnAvailabilitySignal(object sender, EventArgs e) => AvailabilityChanged?.Invoke(this, EventArgs.Empty);

    void OnPriorityChanged(object sender, PriorityChangedEventArgs e) => PriorityChanged?.Invoke(this, e);
}
