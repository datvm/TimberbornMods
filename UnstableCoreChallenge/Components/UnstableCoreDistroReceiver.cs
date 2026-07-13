namespace UnstableCoreChallenge.Components;

[AddTemplateModule2(typeof(UnstableCoreStabilizer))]
public class UnstableCoreDistroReceiver(DistroService service) : DistroComponentBase(service), IDistroReceiver
{
    static readonly ComponentKey SaveKey = new(nameof(UnstableCoreDistroReceiver));
    static readonly PropertyKey<int> PriorityKey = new(nameof(Priority));

    UnstableCoreStabilizer stabilizer = null!;

    public override IEnumerable<GoodAmount> Goods => stabilizer.GetCurrentPayments()
        .Where(p => !p.IsScience && !p.Finished)
        .Select(p => new GoodAmount(p.GoodId, p.Amount - p.Paid));

    public override IEnumerable<string> GoodIds => Goods.Select(g => g.GoodId);

    public Priority Priority { get; private set; } = Priority.Normal;

    public override void Awake()
    {
        base.Awake();
        stabilizer = GetComponent<UnstableCoreStabilizer>();
    }

    public string? CanReceiveGood(HashSet<string> goodIds)
        => Goods.FirstOrDefault(g => goodIds.Contains(g.GoodId)).GoodId;

    protected override bool CalculateActive()
        => base.CalculateActive()
        && Goods.Any();

    public void TransferIn(GoodAmount good)
    {
        stabilizer.Pay(good.GoodId, good.Amount);
        MarkActiveDirty();
    }

    public void SetPriority(Priority priority) => Priority = priority;

    public DistroReceiverSerializableModel Serialize() => new(Enabled, Priority);

    public void Deserialize(DistroReceiverSerializableModel model)
    {
        if (model.Enabled != Enabled)
        {
            SetEnabled(model.Enabled);
        }

        if (model.Priority != Priority)
        {
            SetPriority(model.Priority);
        }
    }

    public override void Save(IEntitySaver entitySaver)
    {
        base.Save(entitySaver);

        var s = entitySaver.GetComponent(SaveKey);
        s.Set(PriorityKey, (int)Priority);
    }

    public override void Load(IEntityLoader entityLoader)
    {
        base.Load(entityLoader);

        if (!entityLoader.TryGetComponent(SaveKey, out var s))
        {
            return;
        }

        if (s.Has(PriorityKey))
        {
            Priority = (Priority)s.Get(PriorityKey);
        }
    }
}
