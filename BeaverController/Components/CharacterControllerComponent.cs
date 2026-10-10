namespace BeaverController.Components;

[AddTemplateModule2(typeof(Character))]
public class CharacterControllerComponent(ReferenceSerializer references) : BaseComponent, IAwakableComponent, IPersistentEntity
{
    static readonly ComponentKey SaveKey = new(nameof(CharacterControllerComponent));
    static readonly PropertyKey<bool> KeepKey = new(nameof(KeepAfterMove));
    static readonly PropertyKey<bool> OrderKey = new(nameof(HasOrder));
    static readonly PropertyKey<Vector3> DestinationKey = new(nameof(OrderDestination));
    static readonly PropertyKey<bool> UseOrderKey = new(nameof(HasUse));
    static readonly PropertyKey<Enterable> UseEnterableKey = new(nameof(UseEnterable));
    static readonly PropertyKey<bool> UseGoodKey = new(nameof(UsesGood));
    static readonly PropertyKey<Inventory> UseInventoryKey = new(nameof(UseInventory));

    Character character = null!;
    ControllableCharacter controllable = null!;
    Walker walker = null!;
    Enterer enterer = null!;
    CharacterModel model = null!;
    BehaviorManager behavior = null!;
    Enterable useEnterable = null!;
    Inventory useInventory = null!;

    public Character Character => character;
    public bool KeepAfterMove { get; private set; }
    public bool HasOrder { get; private set; }
    public bool HasUse { get; private set; }
    public bool UsesGood { get; private set; }
    public Enterable UseEnterable => useEnterable;
    public Inventory UseInventory => useInventory;
    public Vector3 OrderDestination { get; private set; }
    public bool Unreachable { get; private set; }
    public bool IsUnderControl => HasOrder;

    public string? CurrentActionName
    {
        get
        {
            if (!behavior) { return null; }
            return behavior.RunningBehavior.Name;
        }
    }

    public string? StatusKey
    {
        get
        {
            if (Unreachable) { return "LV.BC.Unreachable"; }
            if (HasOrder) { return MoveStatus(); }
            if (HasUse) { return UseStatus(); }
            return null;
        }
    }

    string MoveStatus()
    {
        if (!behavior || !behavior.IsRunningBehavior<CharacterMoveBehavior>())
        {
            return "LV.BC.Waiting";
        }

        if (KeepAfterMove && walker.Stopped())
        {
            return "LV.BC.Held";
        }

        return "LV.BC.Moving";
    }

    string UseStatus()
    {
        if (!behavior || !behavior.IsRunningBehavior<CharacterUseBehavior>())
        {
            return "LV.BC.Waiting";
        }

        if (behavior.IsRunningExecutor<ApplyEffectExecutor>())
        {
            return "LV.BC.Using";
        }

        return "LV.BC.MovingTo";
    }

    public void Awake()
    {
        character = GetComponent<Character>();
        controllable = GetComponent<ControllableCharacter>();
        walker = GetComponent<Walker>();
        enterer = GetComponent<Enterer>();
        model = GetComponent<CharacterModel>();
        behavior = GetComponent<BehaviorManager>();
    }

    public void SetKeep(bool keep)
    {
        KeepAfterMove = keep;
    }

    public void Stop()
    {
        DropTask();
        HasOrder = false;
        Unreachable = false;
        controllable.ReleaseControl();
    }

    public bool CommandUse(BuildingUse use)
    {
        DropTask();
        HasOrder = false;
        Unreachable = false;
        if (use.ConsumesGood)
        {
            if (!use.Inventory || !use.Inventory.HasUnreservedStock(use.Good)) { return false; }

            GetComponent<GoodReserver>().ReserveExactStockAmount(use.Inventory, use.Good);
        }

        if (!use.Enterable || !use.Enterable.CanReserveSlot)
        {
            if (use.ConsumesGood) { GetComponent<GoodReserver>().UnreserveStock(); }
            return false;
        }

        enterer.ReserveSlot(use.Enterable);
        useEnterable = use.Enterable;
        UsesGood = use.ConsumesGood;
        useInventory = use.Inventory;
        var attender = GetComponent<AttractionAttender>();
        if (attender) { attender.FirstVisit = true; }

        HasUse = true;
        controllable.ReleaseControl();
        return true;
    }

    public void CompleteUse()
    {
        HasUse = false;
        UsesGood = false;
        useEnterable = null!;
        useInventory = null!;
        Unreachable = false;
        controllable.ReleaseControl();
    }

    public void FailUse(bool unreachable)
    {
        if (UsesGood && useInventory)
        {
            var reserver = GetComponent<GoodReserver>();
            if (reserver && reserver.StockReservation.Inventory == useInventory)
            {
                reserver.UnreserveStock();
            }
        }

        enterer.UnreserveSlot();
        HasUse = false;
        UsesGood = false;
        useEnterable = null!;
        useInventory = null!;
        Unreachable = unreachable;
        controllable.ReleaseControl();
    }

    public void CommandMove(Vector3 gridDestination)
    {
        DropTask();
        Unreachable = false;
        OrderDestination = gridDestination;
        HasOrder = true;
        controllable.ReleaseControl();
    }

    public void Teleport(Vector3 gridDestination)
    {
        DropTask();
        var world = GridDestinationToWorld(gridDestination);
        DropAt(world);
        Unreachable = false;

        if (!KeepAfterMove)
        {
            CompleteOrder();
            return;
        }

        OrderDestination = CoordinateSystem.WorldToGrid(world);
        HasOrder = true;
    }

    public void Release()
    {
        var ours = behavior && behavior.IsRunningBehavior<CharacterMoveBehavior>();
        HasOrder = false;
        Unreachable = false;
        if (ours)
        {
            walker.StopMoving();
        }

        controllable.ReleaseControl();
    }

    public void BeginHold(string animation)
    {
        controllable.ChangeAnimation(animation);
        controllable.PlayAnimation();
    }

    public void CompleteOrder()
    {
        HasOrder = false;
        controllable.ReleaseControl();
    }

    public void FailOrder()
    {
        HasOrder = false;
        Unreachable = true;
        controllable.ReleaseControl();
    }

    public void Save(IEntitySaver entitySaver)
    {
        if (!KeepAfterMove && !HasOrder && !HasUse) { return; }

        var saver = entitySaver.GetComponent(SaveKey);
        saver.Set(KeepKey, KeepAfterMove);
        saver.Set(OrderKey, HasOrder);
        if (HasOrder)
        {
            saver.Set(DestinationKey, OrderDestination);
        }

        if (!HasUse || !useEnterable) { return; }

        saver.Set(UseOrderKey, true);
        saver.Set(UseGoodKey, UsesGood);
        saver.Set(UseEnterableKey, useEnterable, references.Of<Enterable>());
        if (UsesGood && useInventory)
        {
            saver.Set(UseInventoryKey, useInventory, references.Of<Inventory>());
        }
    }

    public void Load(IEntityLoader entityLoader)
    {
        if (!entityLoader.TryGetComponent(SaveKey, out var loader)) { return; }

        if (loader.Has(KeepKey))
        {
            KeepAfterMove = loader.Get(KeepKey);
        }

        if (loader.Has(OrderKey))
        {
            HasOrder = loader.Get(OrderKey);
        }

        if (loader.Has(DestinationKey))
        {
            OrderDestination = loader.Get(DestinationKey);
        }

        if (!loader.Has(UseOrderKey) || !loader.Get(UseOrderKey)) { return; }
        if (!loader.GetObsoletable(UseEnterableKey, references.Of<Enterable>(), out var enterable)) { return; }

        useEnterable = enterable;
        UsesGood = loader.Has(UseGoodKey) && loader.Get(UseGoodKey);
        if (UsesGood && loader.Has(UseInventoryKey))
        {
            loader.GetObsoletable(UseInventoryKey, references.Of<Inventory>(), out useInventory);
        }

        HasUse = true;
    }

    void DropTask()
    {
        FinishActiveExecutor();
        walker.StopMoving();
        enterer.UnreserveSlotAndExit();

        if (GetComponent<GoodReserver>() is GoodReserver goods)
        {
            goods.UnreserveStock();
            goods.UnreserveCapacity();
        }

        if (behavior is not null)
        {
            behavior._runningExecutor = null;
            behavior._runningBehavior = null;
            behavior._returnToBehavior = false;
            behavior._runningExecutorElapsedTime = 0f;
        }

        HasUse = false;
        UsesGood = false;
        useEnterable = null!;
        useInventory = null!;
    }

    void FinishActiveExecutor()
    {
        if (behavior is null) { return; }

        switch (behavior._runningExecutor)
        {
            case WorkExecutor work when work._isWorking:
                work.StopWorking();
                break;
            case ProduceExecutor produce when produce._isProducing:
                produce.StopProducing();
                break;
            case BuildExecutor build:
                build.StopBuilding();
                break;
            case WorkAtReservableExecutor reservable:
                reservable.Stop();
                break;
            case ApplyEffectExecutor effect:
                effect.TurnOffAnimation();
                break;
        }
    }

    void DropAt(Vector3 world)
    {
        if (enterer.IsInside)
        {
            enterer.Exit();
        }

        walker.StopMoving();
        Transform.position = world;
        model.ResetModelPosition();
        model.Position = world;
    }

    public static Vector3 GridDestinationToWorld(Vector3 grid)
    {
        var tile = NearlyInteger(grid.x) && NearlyInteger(grid.y);
        return tile
            ? CoordinateSystem.GridToWorldCentered(grid)
            : CoordinateSystem.GridToWorld(grid);
    }

    static bool NearlyInteger(float value) => Mathf.Abs(value - Mathf.Round(value)) < 0.001f;
}
