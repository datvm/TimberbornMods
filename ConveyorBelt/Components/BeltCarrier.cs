
namespace ConveyorBelt.Components;

[AddTemplateModule2(typeof(BeltCarrierSpec))]
public class BeltCarrier(BeltCarrierService service)
    : BaseComponent, IBeltConnectionProvider, IBeltTarget, IAwakableComponent, IInitializableEntity, IFinishedStateListener, IPersistentEntity, IEntityDescriber
{
    static readonly ComponentKey SaveKey = new(nameof(BeltCarrier));
    static readonly ListKey<string> ItemsKey = new("Items");
    static readonly PropertyKey<bool> WarnKey = new("Warn");

    readonly List<BeltGood> items = [];
    readonly List<BeltTrail> trails = [];
    readonly List<LocalPort> localPorts = [];

    BlockObject block = null!;
    BlockableObject blockable = null!;
    MechanicalNode mechanical = null!;
    StatusToggle stuckStatus = null!;
    bool accepting;
    float tickHours;

    float? itemsPerHour;
    public float ItemsPerHour => itemsPerHour ??= service.ItemsPerHour(SpeedIndex);

    int? speedIndex;
    public int SpeedIndex => speedIndex ??= service.Index(GetComponent<BeltCarrierSpec>().Speed);

    public BeltShape Shape { get; private set; }
    
    public bool WarnWhenStuck { get; set; }
    public bool IsStuck { get; private set; }
    public Vector3Int Coordinates => block.Coordinates;
    public BeltEnd InPort { get; private set; }
    public BeltEnd OutPort { get; private set; }
    public IReadOnlyList<BeltGood> Items => items;
    public IReadOnlyList<BeltTrail> Trails => trails;
    public bool Running => blockable.IsUnblocked && (!mechanical.IsConsumer || mechanical.PowerEfficiency >= 1f);
    public int Priority => BeltConnectionPriority.Conveyor;
    public int TickGeneration { get; private set; }
    public float TickHours => tickHours;

    public IBeltConnection? Output { get; set; }
    public IBeltConnection? Input { get; set; }

    public void Awake()
    {
        block = GetComponent<BlockObject>();
        blockable = GetComponent<BlockableObject>();
        mechanical = GetComponent<MechanicalNode>();        
    }

    public void InitializeEntity()
    {
        var spec = GetComponent<BeltCarrierSpec>();
        Shape = spec.Shape;
        stuckStatus = StatusToggle.CreateNormalStatusWithAlert(
            "LackOfResources",
            service.t.T("LV.CBlt.Stuck"),
            service.t.T("LV.CBlt.StuckShort"),
            1f);
        GetComponent<StatusSubject>().RegisterStatus(stuckStatus);
        itemsPerHour = service.ItemsPerHour(SpeedIndex);

        RebuildPorts();
    }

    public void OnEnterFinishedState()
    {
        RebuildPorts();
        service.Register(this);
    }

    public void OnExitFinishedState()
    {
        service.Unregister(this);
        Eject();
        stuckStatus.Deactivate();
    }

    public void Eject()
    {
        if (items.Count > 0)
        {
            service.Drop(Coordinates, items);
            items.Clear();
            SetStuck(false);
        }

        trails.Clear();
    }

    public void BeginTick(int generation)
    {
        TickGeneration = generation;
        trails.Clear();
    }

    public void MoveForward(float hoursPerTick)
    {
        tickHours = hoursPerTick;
        if (!Running)
        {
            BeltMotion.Hold(items);
            for (var i = 0; i < items.Count; i++)
            {
                var item = items[i];
                trails.Add(new(item.Id, item.Position, item.Position, 0f, 1f, true));
            }

            SetStuck(false);
            return;
        }

        var stuck = BeltMotion.Advance(
            items,
            BeltRates.Delta(ItemsPerHour, hoursPerTick),
            TickGeneration,
            TryHandOff,
            span => Remember(span, 0f));
        SetStuck(stuck);
    }

    public void PullFromBuilding(float hoursPerTick)
    {
        tickHours = hoursPerTick;
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
            if (!TryAccept(id, BeltTravel.Hours(ItemsPerHour, place), 0f))
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

    public bool TryAccept(string goodId, float leftoverHours, float spentHours)
    {
        if (accepting || !Running || !service.IsCarryable(goodId))
        {
            return false;
        }

        accepting = true;
        try
        {
            return BeltMotion.TryJoin(
                items,
                goodId,
                leftoverHours,
                ItemsPerHour,
                TickGeneration,
                spentHours,
                PassOn,
                span => Remember(span, spentHours));
        }
        finally
        {
            accepting = false;
        }
    }

    void Remember(BeltSpan span, float spentHours)
    {
        var distance = span.To - span.From;
        if (distance < 0f)
        {
            distance = 0f;
        }

        var used = BeltTravel.Hours(ItemsPerHour, distance);
        trails.Add(BeltTrail.Slice(span.Id, span.From, span.To, spentHours, used, tickHours, span.Rest));
    }

    public IEnumerable<EntityDescription> DescribeEntity() => [service.Describe(ItemsPerHour)];

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

    bool PassOn(string goodId, float leftoverHours, float spentHours)
    {
        if (Output is not { } next)
        {
            return false;
        }

        if (!next.TryGetTarget(goodId, out var further) || !further.TryAccept(goodId, leftoverHours, spentHours))
        {
            return false;
        }

        next.Commit();
        return true;
    }

    bool TryHandOff(string goodId, float overshoot, float from)
    {
        if (Output is not { } connection)
        {
            return false;
        }

        var spent = BeltTravel.Hours(ItemsPerHour, BeltRates.End - from);
        if (spent < 0f)
        {
            spent = 0f;
        }

        var hours = BeltTravel.Hours(ItemsPerHour, overshoot);
        if (!connection.TryGetTarget(goodId, out var target) || !target.TryAccept(goodId, hours, spent))
        {
            return false;
        }

        connection.Commit();
        return true;
    }

    void SetStuck(bool stuck)
    {
        IsStuck = stuck;
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
        BeltLayout.FillCarrier(Shape, localPorts);
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
