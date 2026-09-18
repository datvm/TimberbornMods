namespace TimberPipes.Components;

[AddTemplateModule2(typeof(FlowLimitPipeSpec))]
public class FlowLimitPipe : BaseComponent, IAwakableComponent, IPersistentEntity, IDuplicable<FlowLimitPipe>
{
    static readonly ComponentKey SaveKey = new(nameof(FlowLimitPipe));
    static readonly PropertyKey<float> LimitKey = new("LimitM3PerHour");

    IOutflowCounter counter = null!;

    public float LimitM3PerHour { get; set; }

    public bool IsClosed
        => FlowLimitIo.IsClosed(LimitM3PerHour, counter?.HourVolume ?? 0f);

    public float Remaining
        => FlowLimitIo.Remaining(LimitM3PerHour, counter?.HourVolume ?? 0f);

    public void Awake()
    {
        counter = GetComponent<IOutflowCounter>()
            ?? throw new InvalidOperationException($"FlowLimitPipe requires an IOutflowCounter component on the same entity.");
    }

    public void DuplicateFrom(FlowLimitPipe source) => LimitM3PerHour = source.LimitM3PerHour;

    public void Save(IEntitySaver entitySaver)
    {
        if (LimitM3PerHour <= 0f)
        {
            return;
        }

        entitySaver.GetComponent(SaveKey).Set(LimitKey, LimitM3PerHour);
    }

    public void Load(IEntityLoader entityLoader)
    {
        if (!entityLoader.TryGetComponent(SaveKey, out var s) || !s.Has(LimitKey))
        {
            return;
        }

        LimitM3PerHour = Math.Max(0f, s.Get(LimitKey));
    }
}
