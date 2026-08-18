namespace TimberPipes.Components;

[AddTemplateModule2(typeof(BuildingPipeSpec))]
public class BuildingPipe(PipeRegistry registry) : BaseComponent, IAwakableComponent, IInitializableEntity, IPersistentEntity, IFinishedStateListener
{
    public const float MaxWaterHeight = 1.0f;

    static readonly ComponentKey SaveKey = new(nameof(BuildingPipe));
    static readonly PropertyKey<float> WaterHeightKey = new("WaterHeight");

#nullable disable
    BuildingPipeSpec spec;
    BlockObject bo;
#nullable enable

    public FrozenDictionary<PipePortDefinition, PipePort>? Ports { get; private set; }
    public PipeGraph? Graph { get; internal set; }

    public float WaterHeight { get; private set; }
    public bool IsFinished => bo.IsFinished;
    public bool IsTransportPipe { get; private set; }
    public Vector3Int Coordinates => bo.Coordinates;

    public void Awake()
    {
        spec = GetComponent<BuildingPipeSpec>();
        bo = GetComponent<BlockObject>();

        IsTransportPipe = HasComponent<TransportPipeSpec>();
    }

    public void InitializeEntity() => InitializePorts();

    void InitializePorts()
    {
        Dictionary<PipePortDefinition, PipePort> ports = [];

        foreach (var portSpec in spec.Ports)
        {
            foreach (var d in portSpec.Directions)
            {
                var def = new PipePortDefinition(
                    bo.TransformCoordinates(portSpec.Coordinates),
                    d
                );

                if (ports.ContainsKey(def))
                {
                    throw new InvalidOperationException($"{spec.Blueprint}: Duplicate port definition found at coordinates {def.Coordinates} with direction {def.Direction}");
                }

                ports[def] = new(def, portSpec);
            }
        }

        if (ports.Count < 1)
        {
            throw new InvalidOperationException($"{spec.Blueprint}: No ports defined");
        }

        Ports = ports.ToFrozenDictionary();
    }

    public void AddWater(float amount) => SetWaterHeight(WaterHeight + amount);
    public void RemoveWater(float amount) => SetWaterHeight(WaterHeight - amount);
    void SetWaterHeight(float height) => WaterHeight = Math.Clamp(height, 0, MaxWaterHeight);

    public void Save(IEntitySaver entitySaver)
    {
        var s = entitySaver.GetComponent(SaveKey);
        s.Set(WaterHeightKey, WaterHeight);
    }

    public void Load(IEntityLoader entityLoader)
    {
        if (!entityLoader.TryGetComponent(SaveKey, out var s)) { return; }
        WaterHeight = s.Get(WaterHeightKey);
    }

    public void OnEnterFinishedState() => registry.Register(this);
    public void OnExitFinishedState() => registry.Unregister(this);
}
