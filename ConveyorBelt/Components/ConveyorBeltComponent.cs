namespace ConveyorBelt.Components;

[AddTemplateModule2(typeof(ConveyorBeltSpec))]
public class ConveyorBeltComponent(ConveyorBeltService service)
    : BaseComponent, IAwakableComponent, IFinishedStateListener, IEntityDescriber, IInitializableEntity, IPersistentEntity
{
    static readonly ComponentKey SaveKey = new(nameof(ConveyorBeltComponent));
    static readonly ListKey<string> ItemsKey = new("Items");

    public ConveyorBeltSpec Spec { get; private set; } = null!;
    BlockObject bo = null!;
    MechanicalBuilding mechanicalBuilding = null!;

    public Vector3Int Coordinates => bo.Coordinates;

    readonly List<ConveyorBeltItem> items = [];
    public IReadOnlyList<ConveyorBeltItem> Items => items;

    public Vector3Int InputCoordinates { get; private set; }
    public Vector3Int OutputCoordinates { get; private set; }
    
    public ConveyorBeltItem? Head => items.Count > 0 ? items[0] : null;
    public ConveyorBeltItem? Tail => items.Count > 0 ? items[^1] : null;
    public float EndPosition => 1f;
    public float ItemSpace => 1f / Spec.Capacity;

    public bool CanAcceptItem
    {
        get
        {
            if (!mechanicalBuilding.CanUse) { return false; }
            if (Tail is not { } t) { return true; }
            return items.Count < Spec.Capacity && t.Position >= ItemSpace;
        }
    }

    public bool HasItemAtTheEnd
    {
        get
        {
            if (Head is not { } h) { return false; }
            return h.Position >= EndPosition;
        }
    }

    public bool IsInputCoordinates(Vector3Int coords) => InputCoordinates == coords;
    public bool IsOutputCoordinates(Vector3Int coords) => OutputCoordinates == coords;

    public void Awake()
    {
        Spec = GetComponent<ConveyorBeltSpec>();
        bo = GetComponent<BlockObject>();
        mechanicalBuilding = GetComponent<MechanicalBuilding>();
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
        if (items.Count == 0) { return; }

        service.SpawnGoods(Coordinates, items.Select(i => new GoodAmount(i.GoodId, 1)));
        items.Clear();
    }

    public void Push(string goodId)
    {
        if (!CanAcceptItem)
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

        var throughput = spec.Capacity / spec.TravelTimeHours;

        return [ EntityDescription.CreateTextSection($"""
            {SpecialStrings.RowStarter} {t.T("LV.CBlt.TravelTime", spec.TravelTimeHours, throughput)}
            {SpecialStrings.RowStarter} {t.T("LV.CBlt.Capacity", spec.Capacity)}
            """, 2001)
        ];
    }

    public void Save(IEntitySaver entitySaver)
    {
        var s = entitySaver.GetComponent(SaveKey);
        s.Set(ItemsKey, [.. items.Select(i => i.Serialize())]);
    }

    public void Load(IEntityLoader entityLoader)
    {
        if (!entityLoader.TryGetComponent(SaveKey, out var s)) { return; }

        if (s.Has(ItemsKey))
        {
            var serializedItems = s.Get(ItemsKey);
            items.Clear();
            items.AddRange(serializedItems.Select(ConveyorBeltItem.Deserialize));
        }
    }
}
