namespace TimberPipes.Components;

[AddTemplateModule2(typeof(BuildingPipeSpec))]
public class BuildingPipe(PipeRegistry registry) : BaseComponent, IAwakableComponent, IInitializableEntity, IPersistentEntity, IFinishedStateListener
{
    public const float MaxWaterHeight = 1.0f;
    public const string FluidGoodIdContaminated = "_CONTAMINATED_PIPE_";

    static readonly ComponentKey SaveKey = new(nameof(BuildingPipe));
    static readonly PropertyKey<float> WaterHeightKey = new("FluidHeight");
    static readonly PropertyKey<string> FluidGoodIdKey = new("FluidGoodId");

#nullable disable
    BuildingPipeSpec spec;
    BlockObject bo;
#nullable enable

    public FrozenDictionary<PipePortDefinition, PipePort>? Ports { get; private set; }
    public PipeGraph? Graph { get; internal set; }

    public string? FluidGoodId { get; private set; }
    public float FluidHeight { get; private set; }
    public bool IsContaminated => FluidGoodId == FluidGoodIdContaminated;

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

    public void AddFluid(string id, float amount)
    {
        if (IsContaminated)
        {
            throw new InvalidOperationException($"Cannot add fluid to contaminated pipe at {Coordinates}");
        }

        if (FluidGoodId is null || FluidGoodId == id)
        {
            SetWaterHeight(FluidHeight + amount);
        }
        else
        {
            // Contaminate the graph
        }
    }

    public void RemoveFluid(float amount) => SetWaterHeight(FluidHeight - amount);
    void SetWaterHeight(float height) => FluidHeight = Math.Clamp(height, 0, MaxWaterHeight);

    public void Save(IEntitySaver entitySaver)
    {
        var s = entitySaver.GetComponent(SaveKey);
        s.Set(WaterHeightKey, FluidHeight);
        s.Set(FluidGoodIdKey, FluidGoodId ?? "");
    }

    public void Load(IEntityLoader entityLoader)
    {
        if (!entityLoader.TryGetComponent(SaveKey, out var s)) { return; }
        FluidHeight = s.Get(WaterHeightKey);

        var id = s.Get(FluidGoodIdKey);
        FluidGoodId = id is null || id.Length == 0 ? null : id;
    }

    public void OnEnterFinishedState() => registry.Register(this);
    public void OnExitFinishedState() => registry.Unregister(this);
}
