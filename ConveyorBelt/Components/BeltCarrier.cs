
namespace ConveyorBelt.Components;

[AddTemplateModule2(typeof(BeltCarrierSpec))]
public class BeltCarrier(BeltCarrierService service)
    : BaseComponent, IBeltConnectionProvider, IBeltTarget, IAwakableComponent, IInitializableEntity, IFinishedStateListener, IPersistentEntity, IEntityDescriber
{
    static readonly ComponentKey SaveKey = new(nameof(BeltCarrier));
    static readonly ListKey<string> ItemsKey = new("Items");
    static readonly PropertyKey<bool> WarnKey = new("Warn");

    readonly List<BeltGood> items = [];
    readonly List<LocalPort> localPorts = [];

    BlockObject block = null!;
    MechanicalNode mechanical = null!;
    StatusToggle stuckStatus = null!;
    bool accepting;

    public BeltShape Shape { get; private set; }
    public string Speed { get; private set; } = "";
    public float ItemsPerHour { get; private set; }
    public bool WarnWhenStuck { get; set; }
    public Vector3Int Coordinates => block.Coordinates;
    public BeltEnd InPort { get; private set; }
    public BeltEnd OutPort { get; private set; }
    public IReadOnlyList<BeltGood> Items => items;
    public bool Running => !mechanical.IsConsumer || mechanical.PowerEfficiency >= 1f;
    public int Priority => BeltConnectionPriority.Conveyor;
    public int TickGeneration { get; private set; }

    public IBeltConnection? Output { get; set; }
    public IBeltConnection? Input { get; set; }

    public void Awake()
    {
        block = GetComponent<BlockObject>();
        mechanical = GetComponent<MechanicalNode>();
        var spec = GetComponent<BeltCarrierSpec>();
        Shape = spec.Shape;
        Speed = spec.Speed;
        ItemsPerHour = service.ItemsPerHour(spec.Speed);
        stuckStatus = StatusToggle.CreateNormalStatusWithAlert(
            "LackOfResources",
            service.t.T("LV.CBlt.Stuck"),
            service.t.T("LV.CBlt.StuckShort"),
            1f);
        GetComponent<StatusSubject>().RegisterStatus(stuckStatus);
    }

    public void InitializeEntity() => RebuildPorts();

    public void OnEnterFinishedState()
    {
        RebuildPorts();
        service.Register(this);
    }

    public void OnExitFinishedState()
    {
        service.Unregister(this);
        service.Drop(Coordinates, items);
        items.Clear();
        stuckStatus.Deactivate();
    }

    public void BeginTick(int generation) => TickGeneration = generation;

    public void MoveForward(float hoursPerTick)
    {
        if (!Running)
        {
            SetStuck(false);
            return;
        }

        var stuck = BeltMotion.Advance(items, BeltRates.Delta(ItemsPerHour, hoursPerTick), TickGeneration, TryHandOff);
        SetStuck(stuck);
    }

    public void PullFromBuilding(float hoursPerTick)
    {
        if (!Running || Input is not IBeltSource source)
        {
            return;
        }

        var place = BeltTravel.Distance(ItemsPerHour, hoursPerTick);
        var guard = 0;
        while (guard++ < 64 && place >= 0f)
        {
            if (items.Count >= BeltRates.Capacity)
            {
                return;
            }

            if (items.Count > 0)
            {
                var room = items[^1].Position - BeltRates.Spacing;
                if (room < 0f)
                {
                    return;
                }

                if (place > room)
                {
                    place = room;
                }
            }

            if (!source.TryTake(out var id))
            {
                return;
            }

            var before = items.Count;
            if (!TryAccept(id, BeltTravel.Hours(ItemsPerHour, place)))
            {
                return;
            }

            if (items.Count > before)
            {
                place = items[^1].Position - BeltRates.Spacing;
            }
            else
            {
                place -= BeltRates.Spacing;
            }
        }
    }

    public bool TryProvide(BeltApproach approach, out IBeltConnection connection)
    {
        connection = MissingConnection.Instance;
        if (!block.IsFinished || approach.To != Coordinates || approach.From != InPort.Target)
        {
            return false;
        }

        connection = new DirectBeltConnection(this);
        return true;
    }

    public bool CanAccept(string goodId) => Running && service.IsCarryable(goodId) && BeltMotion.CanAccept(items);

    public bool TryAccept(string goodId, float leftoverHours)
    {
        if (accepting || !Running || !service.IsCarryable(goodId))
        {
            return false;
        }

        accepting = true;
        try
        {
            var distance = BeltTravel.Distance(ItemsPerHour, leftoverHours);
            if (items.Count == 0 && distance >= BeltRates.End && Output is { } next)
            {
                var left = leftoverHours - BeltTravel.Hours(ItemsPerHour, BeltRates.End);
                if (left < 0f)
                {
                    left = 0f;
                }

                if (next.TryGetTarget(goodId, out var further) && further.TryAccept(goodId, left))
                {
                    next.Commit();
                    return true;
                }
            }

            if (!BeltMotion.TryArrival(items, distance, out var position))
            {
                return false;
            }

            items.Add(new(goodId, position, TickGeneration));
            return true;
        }
        finally
        {
            accepting = false;
        }
    }

    public IEnumerable<EntityDescription> DescribeEntity() => [service.Describe(Speed, ItemsPerHour)];

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

    bool TryHandOff(string goodId, float overshoot)
    {
        if (Output is not { } connection)
        {
            return false;
        }

        var hours = BeltTravel.Hours(ItemsPerHour, overshoot);
        if (!connection.TryGetTarget(goodId, out var target) || !target.TryAccept(goodId, hours))
        {
            return false;
        }

        connection.Commit();
        return true;
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
