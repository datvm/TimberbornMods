namespace ConveyorBelt.Components;

[AddTemplateModule2(typeof(BeltCarrierSpec))]
public class BeltCarrier(BeltGoodService goods, BeltRegistry registry, ConveyorBeltSpeeds speeds, FactionService factions, ILoc t) : BaseComponent, IAwakableComponent, IInitializableEntity, IFinishedStateListener, IPersistentEntity, IEntityDescriber
{
    static readonly ComponentKey SaveKey = new(nameof(BeltCarrier));
    static readonly ListKey<string> ItemsKey = new("Items");
    static readonly PropertyKey<bool> WarnKey = new("Warn");

    readonly List<BeltGood> items = [];
    readonly List<LocalPort> localPorts = [];

    BlockObject block = null!;
    MechanicalNode mechanical = null!;
    StatusToggle stuckStatus = null!;
    float traveled;

    public BeltShape Shape { get; private set; }
    public string Speed { get; private set; } = "";
    public float ItemsPerHour { get; private set; }
    public bool LinksBuildings { get; private set; }
    public bool WarnWhenStuck { get; set; }
    public Vector3Int Coordinates => block.Coordinates;
    public BeltEnd InPort { get; private set; }
    public BeltEnd OutPort { get; private set; }
    public IReadOnlyList<BeltGood> Items => items;
    public bool HeadReady => BeltMotion.HeadReady(items);
    public bool Running => !mechanical.IsConsumer || mechanical.PowerEfficiency >= 1f;

    public BeltCarrier? OutputCarrier { get; set; }
    public BlockObject? OutputBuilding { get; set; }
    public Inventories? OutputInventories { get; set; }
    public BlockObject? InputBuilding { get; set; }
    public Inventories? InputInventories { get; set; }

    public void Awake()
    {
        block = GetComponent<BlockObject>();
        mechanical = GetComponent<MechanicalNode>();
        var spec = GetComponent<BeltCarrierSpec>();
        Shape = spec.Shape;
        Speed = spec.Speed;
        ItemsPerHour = speeds.ItemsPerHour(spec.Speed);
        LinksBuildings = BeltShapeInfo.LinksBuildings(spec.Shape);
        stuckStatus = StatusToggle.CreateNormalStatusWithAlert(
            "LackOfResources",
            t.T("LV.CBlt.Stuck"),
            t.T("LV.CBlt.StuckShort"),
            1f);
        GetComponent<StatusSubject>().RegisterStatus(stuckStatus);
    }

    public void InitializeEntity() => RebuildPorts();

    public void OnEnterFinishedState()
    {
        RebuildPorts();
        registry.Register(this);
    }

    public void OnExitFinishedState()
    {
        registry.Unregister(this);
        goods.Drop(Coordinates, items);
        items.Clear();
        stuckStatus.Deactivate();
    }

    public void MoveForward(float hoursPerTick)
    {
        if (!Running)
        {
            traveled = 0f;
            SetStuck(false);
            return;
        }

        traveled = BeltRates.Delta(ItemsPerHour, hoursPerTick);
        var stuck = BeltMotion.Advance(items, traveled, TryHandOff);
        SetStuck(stuck);
    }

    public void PullFromBuilding()
    {
        if (!Running || !InputBuilding || !InputInventories)
        {
            return;
        }

        while (BeltMotion.TryArrival(items, traveled, out var position))
        {
            if (!goods.TryTake(InputBuilding!, InputInventories!, out var id))
            {
                return;
            }

            items.Add(new(id, position));
        }
    }

    public bool TryReceive(string goodId)
    {
        if (!Running || !goods.IsCarryable(goodId) || !BeltMotion.TryArrival(items, traveled, out var position))
        {
            return false;
        }

        items.Add(new(goodId, position));
        return true;
    }

    public bool CanTake(string goodId) => Running && goods.IsCarryable(goodId) && BeltMotion.CanAccept(items);

    public void Push(string goodId) => items.Add(new(goodId, 0f));

    public bool TryPop(out string goodId)
    {
        goodId = "";
        if (!HeadReady)
        {
            return false;
        }

        goodId = items[0].Id;
        items.RemoveAt(0);
        SetStuck(false);
        return true;
    }

    public bool OutTargets(Vector3Int cell) => OutPort.Target == cell;

    public bool InTargets(Vector3Int cell) => InPort.Target == cell;

    public IEnumerable<EntityDescription> DescribeEntity()
    {
        var tier = t.T(speeds.NameKey(Speed, factions.Current.Id));
        var line = t.T("LV.CBlt.TierRate", tier, ItemsPerHour, BeltRates.Capacity);
        return [EntityDescription.CreateTextSection($"{SpecialStrings.RowStarter} {line}", 200)];
    }

    public void Save(IEntitySaver saver)
    {
        var s = saver.GetComponent(SaveKey);
        if (items.Count > 0)
        {
            List<string> serialized = [];
            foreach (var item in items)
            {
                serialized.Add(item.Serialize());
            }

            s.Set(ItemsKey, serialized);
        }

        if (WarnWhenStuck)
        {
            s.Set(WarnKey, true);
        }
    }

    public void Load(IEntityLoader loader)
    {
        if (!loader.TryGetComponent(SaveKey, out var s))
        {
            return;
        }

        if (s.Has(ItemsKey))
        {
            items.Clear();
            foreach (var text in s.Get(ItemsKey))
            {
                if (BeltGood.TryDeserialize(text, out var good))
                {
                    items.Add(good);
                }
            }
        }

        if (s.Has(WarnKey))
        {
            WarnWhenStuck = s.Get(WarnKey);
        }
    }

    bool TryHandOff(string goodId)
    {
        if (OutputCarrier is { } next && next.TryReceive(goodId))
        {
            return true;
        }

        if (OutputBuilding is not { } building || OutputInventories is not { } inventories)
        {
            return false;
        }

        return goods.TryGive(building, inventories, goodId);
    }

    void SetStuck(bool stuck)
    {
        if (WarnWhenStuck && stuck)
        {
            stuckStatus.Activate();
        }
        else
        {
            stuckStatus.Deactivate();
        }
    }

    void RebuildPorts()
    {
        BeltLayout.FillCarrier(Shape, block.FlipMode.IsFlipped, localPorts);
        foreach (var port in localPorts)
        {
            var end = new BeltEnd(block.TransformCoordinates(Vector3Int.zero), block.TransformDirection(port.Direction));
            if (port.Incoming)
            {
                InPort = end;
            }
            else
            {
                OutPort = end;
            }
        }
    }
}
