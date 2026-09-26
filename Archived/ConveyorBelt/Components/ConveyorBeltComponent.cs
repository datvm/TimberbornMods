namespace ConveyorBelt.Components;

[AddTemplateModule2(typeof(ConveyorBeltSpec))]
public class ConveyorBeltComponent(ConveyorBeltService service)
    : BaseComponent, IAwakableComponent, IFinishedStateListener, IEntityDescriber, IInitializableEntity, IPersistentEntity
{
    static readonly ComponentKey SaveKey = new(nameof(ConveyorBeltComponent));
    static readonly ListKey<string> ItemsKey = new("Items");
    static readonly PropertyKey<string> FilteredGoodIdKey = new("FilteredGoodId");

#nullable disable
    public ConveyorBeltSpec Spec { get; private set; }
    BlockObject bo;
    MechanicalNode mechanicalNode;
    ConveyorConnection connection;
#nullable enable

    string forbiddenText = "";
    readonly HashSet<string> forbiddenGoodTypes = [];
    readonly List<ConveyorBeltItem> items = [];

    public Vector3Int Coordinates => bo.Coordinates;
    public bool CanFilterItems => Spec.CanFilterItem;
    public bool NeedsPower => mechanicalNode.IsConsumer;
    public bool CanUse => !NeedsPower || mechanicalNode.ActiveAndPowered;
    public float Efficiency => NeedsPower ? mechanicalNode.PowerEfficiency : 1f;
    public ConveyorConnection Connection => connection;
    public IReadOnlyList<ConveyorBeltItem> Items => items;

    public Vector3Int InputCoordinates { get; private set; }
    public Vector3Int OutputCoordinates { get; private set; }

    public string? FilteredGoodId { get; set; }

    public ConveyorBeltItem? Head => items.Count > 0 ? items[0] : null;
    public ConveyorBeltItem? Tail => items.Count > 0 ? items[^1] : null;
    public float EndPosition => 1f;
    public float ItemSpace => 1f / Spec.Capacity;

    public bool CanAcceptItem(string goodId)
        => CanAcceptPotentialItem() && IsValidGood(goodId);

    public bool CanAcceptPotentialItem()
    {
        if (!CanUse)
        {
            return false;
        }

        if (Tail is { } t)
        {
            if (items.Count >= Spec.Capacity || t.Position < ItemSpace)
            {
                return false;
            }
        }

        return true;
    }

    public bool IsValidGood(string goodId) =>
        (!Spec.CanFilterItem || FilteredGoodId is null || FilteredGoodId == goodId)
        && !ForbidsGoodType(service.GetGoodType(goodId));

    public bool ForbidsGoodType(string goodType) => forbiddenGoodTypes.Contains(goodType);

    public bool CanGiveItem
    {
        get
        {
            if (Head is not { } h)
            {
                return false;
            }

            return h.Position >= EndPosition;
        }
    }

    public bool IsInputCoordinates(Vector3Int coords) => InputCoordinates == coords;
    public bool IsOutputCoordinates(Vector3Int coords) => OutputCoordinates == coords;

    public void Awake()
    {
        Spec = GetComponent<ConveyorBeltSpec>();
        bo = GetComponent<BlockObject>();
        mechanicalNode = GetComponent<MechanicalNode>();
        connection = GetComponent<ConveyorConnection>();

        forbiddenGoodTypes.Add(ConveyorBeltService.LiquidGoodType);
        foreach (var type in Spec.ForbiddenGoodTypes)
        {
            forbiddenGoodTypes.Add(type);
        }

        var t = service.t;
        var list = string.Join(", ", forbiddenGoodTypes.Select(g => t.T("LV.CBlt.GoodType_" + g)));
        forbiddenText = $"{Environment.NewLine}{SpecialStrings.RowStarter} {t.T("LV.CBlt.CannotCarry", list)}";
    }

    public void InitializeEntity()
    {
        InputCoordinates = bo.TransformCoordinates(Spec.InputCoordinates);
        OutputCoordinates = bo.TransformCoordinates(Spec.OutputCoordinates);
    }

    public void OnEnterFinishedState()
    {
        service.RegisterBelt(this);
    }

    public void OnExitFinishedState()
    {
        service.UnregisterBelt(this);
        EjectContent();
    }

    public void EjectContent()
    {
        if (items.Count == 0)
        {
            return;
        }

        service.SpawnGoods(Coordinates, items.Select(i => new GoodAmount(i.GoodId, 1)));
        items.Clear();
    }

    public void Push(string goodId)
    {
        if (!CanAcceptItem(goodId))
        {
            throw new InvalidOperationException("Cannot accept item at this time.");
        }

        items.Add(new(goodId));
    }

    public ConveyorBeltItem Pop(bool ignoreTravelTime = false)
    {
        if (Head is not { } h)
        {
            throw new InvalidOperationException("No item to pop.");
        }

        if (!ignoreTravelTime && h.Position < EndPosition)
        {
            throw new InvalidOperationException("Item is not ready to be popped.");
        }

        items.RemoveAt(0);
        return h;
    }

    public IEnumerable<EntityDescription> DescribeEntity()
    {
        var t = service.t;
        var spec = Spec;
        var speed = service.SpeedMultiplier;
        var travelHours = spec.TravelTimeHours;
        var throughput = spec.Capacity / travelHours * speed;
        if (speed > 0)
        {
            travelHours /= speed;
        }

        return [ EntityDescription.CreateTextSection($"""
            {SpecialStrings.RowStarter} {t.T("LV.CBlt.TravelTime", travelHours, throughput)}
            {SpecialStrings.RowStarter} {t.T("LV.CBlt.Capacity", spec.Capacity)}{forbiddenText}
            """, 2001)
        ];
    }

    public void Save(IEntitySaver entitySaver)
    {
        var s = entitySaver.GetComponent(SaveKey);
        s.Set(ItemsKey, [.. items.Select(i => i.Serialize())]);

        if (FilteredGoodId is not null)
        {
            s.Set(FilteredGoodIdKey, FilteredGoodId);
        }
    }

    public void Load(IEntityLoader entityLoader)
    {
        if (!entityLoader.TryGetComponent(SaveKey, out var s))
        {
            return;
        }

        if (s.Has(ItemsKey))
        {
            var serializedItems = s.Get(ItemsKey);
            items.Clear();
            items.AddRange(serializedItems.Select(ConveyorBeltItem.Deserialize));
        }

        if (s.Has(FilteredGoodIdKey))
        {
            FilteredGoodId = s.Get(FilteredGoodIdKey);
        }
    }
}
