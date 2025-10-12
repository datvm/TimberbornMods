namespace BeavlineLogistics.Components;

public class BeavlineComponent : BaseComponent, IPersistentEntity, IDeletableEntity
{
    static readonly ComponentKey SaveKey = new(nameof(BeavlineComponent));
    static readonly PropertyKey<bool> DisableInputKey = new("DisableInput");
    static readonly ListKey<string> FilteredInputKey = new("FilteredInput");
    static readonly PropertyKey<bool> DisableOutputKey = new("DisableOutput");
    static readonly ListKey<string> FilteredOutputKey = new("FilteredOutput");

    public string PrefabName { get; private set; } = "";

    public Inventory? InputInventory { get; private set; }
    public FrozenSet<string> InputGoodIds { get; private set; } = [];
    public bool CanTakeIn => InputInventory is not null; // Is not null is good enough because we set it to null if there are no takable goods.

    public Inventory? OutputInventory { get; private set; }
    public FrozenSet<string> OutputGoodIds { get; private set; } = [];
    public bool CanGiveOut => OutputInventory is not null;

    public bool CanHaveBeavline => CanTakeIn || CanGiveOut;
    public bool HasAllBeavlines => (!CanTakeIn || HasInput) && (!CanGiveOut || HasOutput); // Don't use == because it's possible it get to true before

    public bool HasInput { get; private set; }
    public bool HasOutput { get; private set; }
    public bool Active => HasInput || HasOutput;

    readonly List<BeavlineComponent> connectedBuildings = [];
    public IReadOnlyList<BeavlineComponent> ConnectedBuildings => connectedBuildings;
    public bool DisableInput { get; set; }
    public FrozenSet<string>? FilteredInput { get; set; }
    public bool DisableOutput { get; private set; }
    public FrozenSet<string>? FilteredOutput { get; set; }

#nullable disable
    public BeavlineService BeavlineService { get; private set; }
#nullable enable
    public BeavlineOutputComponent? BeavlineOutput { get; private set; }

    [Inject]
    public void Inject(BeavlineService service)
    {
        this.BeavlineService = service;
    }

    public HashSet<string> GetOutputGoods()
    {
        if (!HasOutput || DisableOutput || OutputInventory is null) { return []; }

        HashSet<string> result = [];
        var filtered = FilteredOutput;

        foreach (var g in OutputInventory.UnreservedTakeableStock())
        {
            if ((filtered is not null && !filtered.Contains(g.GoodId))
                || g.Amount <= 0)
            {
                continue;
            }

            result.Add(g.GoodId);
        }

        return result;
    }

    public HashSet<string> GetPossibleInputGoods(HashSet<string> goods)
    {
        if (!HasInput || DisableInput || InputInventory is null) { return []; }

        HashSet<string> result = [];
        var filtered = FilteredInput;
        foreach (var g in goods)
        {
            if ((filtered is not null && !filtered.Contains(g))
                || InputInventory.UnreservedCapacity(g) <= 0)
            {
                continue;
            }

            result.Add(g);
        }
        return result;
    }

    public void Start()
    {
        PrefabName = GetComponentFast<PrefabSpec>().PrefabName;
        GetInventory();

        if (!CanHaveBeavline) { return; }
        CheckBuiltBeavline();
    }

    void GetInventory()
    {
        var inventories = BeavlineService.BuildingInventoryProvider.Get(this);
        if (inventories.InventoryFunc is null) { return; }

        inventories.InventoryFunc(this, out var input, out var output);

        InputGoodIds = inventories.InputIds;
        if (InputGoodIds.Count > 0)
        {
            InputInventory = input;
        }

        OutputGoodIds = inventories.OutputIds;
        if (OutputGoodIds.Count > 0)
        {
            OutputInventory = output;
        }
    }

    void CheckBuiltBeavline()
    {
        var reno = this.GetRenovationComponent();

        HasInput = CanTakeIn && reno.HasRenovation(BeavlineIORenovationProvider.RenovationInId);
        HasOutput = CanGiveOut && reno.HasRenovation(BeavlineIORenovationProvider.RenovationOutId);

        if (HasOutput)
        {
            Activate(false);
        }

        if (HasInput)
        {
            Activate(true);
        }

        if (!HasAllBeavlines)
        {
            reno.RenovationCompleted += OnRenoCompleted;
        }
    }

    internal void AddConnectedBuilding(BeavlineComponent comp)
    {
        connectedBuildings.Add(comp);
    }

    internal void RemoveConnectedBuilding(BeavlineComponent comp)
    {
        connectedBuildings.Remove(comp);
    }

    private void OnRenoCompleted(BuildingRenovation proj)
    {
        switch (proj.Id)
        {
            case BeavlineIORenovationProvider.RenovationInId:
                Activate(true);
                break;
            case BeavlineIORenovationProvider.RenovationOutId:
                Activate(false);
                break;
        }
    }

    void Activate(bool input)
    {
        if (input)
        {
            HasInput = true;
        }
        else
        {
            HasOutput = true;
        }
        BeavlineService.Register(this);

        if (!input)
        {
            BeavlineOutput = GetComponentFast<BeavlineOutputComponent>();
            BeavlineOutput.enabled = true;
            SetOutputDisabled(DisableOutput);
        }

        if (HasAllBeavlines)
        {
            var reno = this.GetRenovationComponent();
            reno.RenovationCompleted -= OnRenoCompleted;
        }
    }

    public void SetOutputDisabled(bool disabled)
    {
        DisableOutput = disabled;
        BeavlineOutput!.Toggle(!disabled);
    }

    public void Save(IEntitySaver entitySaver)
    {
        if (!CanHaveBeavline || !Active) { return; }

        var s = entitySaver.GetComponent(SaveKey);
        if (DisableInput)
        {
            s.Set(DisableInputKey, true);
        }
        if (FilteredInput is not null && FilteredInput.Count > 0)
        {
            s.Set(FilteredInputKey, FilteredInput);
        }
        if (DisableOutput)
        {
            s.Set(DisableOutputKey, true);
        }
        if (FilteredOutput is not null && FilteredOutput.Count > 0)
        {
            s.Set(FilteredOutputKey, FilteredOutput);
        }
    }

    public void Load(IEntityLoader entityLoader)
    {
        if (!entityLoader.TryGetComponent(SaveKey, out var s)) { return; }

        if (s.Has(DisableInputKey))
        {
            DisableInput = s.Get(DisableInputKey);
        }
        if (s.Has(FilteredInputKey))
        {
            FilteredInput = [.. s.Get(FilteredInputKey)];
            if (FilteredInput.Count == 0) { FilteredInput = null; }
        }
        if (s.Has(DisableOutputKey))
        {
            DisableOutput = s.Get(DisableOutputKey);
        }
        if (s.Has(FilteredOutputKey))
        {
            FilteredOutput = [.. s.Get(FilteredOutputKey)];
            if (FilteredOutput.Count == 0) { FilteredOutput = null; }
        }
    }

    public void DeleteEntity()
    {
        BeavlineService.Unregister(this);
    }
}
