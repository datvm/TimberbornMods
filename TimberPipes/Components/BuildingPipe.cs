namespace TimberPipes.Components;

[AddTemplateModule2(typeof(BuildingPipeSpec))]
[AddTemplateModule2(typeof(IGeneratedBuildingPipeComponent), AlsoBindTransient = false)]
public class BuildingPipe(PipeRegistry registry) : BaseComponent, IAwakableComponent, IInitializableEntity, IPersistentEntity, IFinishedStateListener
{
    public const float MaxWaterHeight = PipeFluids.PipeCapacity;
    public const string FluidGoodIdContaminated = PipeFluids.ContaminatedId;

    static readonly ComponentKey SaveKey = new(nameof(BuildingPipe));
    static readonly PropertyKey<float> WaterHeightKey = new("FluidHeight");
    static readonly PropertyKey<string> FluidGoodIdKey = new("FluidGoodId");
    static readonly PropertyKey<string> MixGoodAKey = new("MixGoodA");
    static readonly PropertyKey<string> MixGoodBKey = new("MixGoodB");

#nullable disable
    BlockObject bo;
#nullable enable

    public BuildingPipeSpec? Spec { get; private set; }
    public FrozenDictionary<PipePortDefinition, PipePort>? Ports { get; private set; }
    public PipeGraph? Graph { get; internal set; }

    public string? FluidGoodId { get; private set; }
    public float FluidHeight { get; private set; }
    public bool IsContaminated => FluidGoodId == FluidGoodIdContaminated;
    public string? NetworkGoodId
        => IsContaminated || Graph is { Contaminated: true }
            ? null
            : Graph?.FluidGoodId ?? FluidGoodId;
    public PipeContaminationCause ContaminationCause { get; private set; }

    public bool IsFinished => bo.IsFinished;
    public bool IsTransportPipe { get; private set; }
    public Vector3Int Coordinates => bo.Coordinates;
    public float Head => Coordinates.z + FluidHeight;
    public float FreeSpace => MaxWaterHeight - FluidHeight;

    public void Awake()
    {
        bo = GetComponent<BlockObject>();
        IsTransportPipe = HasComponent<TransportPipeSpec>();
        Spec = ResolveSpec();
        if (Spec is null)
        {
            DisableComponent();
        }
    }

    public void InitializeEntity()
    {
        if (Spec is null)
        {
            return;
        }

        InitializePorts();
    }

    BuildingPipeSpec? ResolveSpec()
    {
        if (TryGetComponent<BuildingPipeSpec>(out var declared) && declared.Ports.Length > 0)
        {
            return declared;
        }

        List<IGeneratedBuildingPipeComponent> generators = [];
        GetComponents(generators);
        foreach (var generator in generators)
        {
            var generated = generator.GetBuildingPipeSpec();
            if (generated is { Ports.Length: > 0 })
            {
                return generated;
            }
        }

        return null;
    }

    void InitializePorts()
    {
        Dictionary<PipePortDefinition, PipePort> ports = [];
        var spec = Spec!;

        var directed = HasComponent<DirectedPipeSpec>();
        foreach (var portSpec in spec.Ports)
        {
            var oriented = portSpec with
            {
                State = directed
                    ? DirectedPipeOrient.State(portSpec, bo.FlipMode.IsFlipped)
                    : portSpec.State,
            };

            foreach (var d in oriented.Directions)
            {
                var def = new PipePortDefinition(
                    bo.TransformCoordinates(oriented.Coordinates),
                    bo.TransformDirection(d)
                );

                if (ports.ContainsKey(def))
                {
                    throw new InvalidOperationException($"{Name}: Duplicate port definition found at coordinates {def.Coordinates} with direction {def.Direction}");
                }

                ports[def] = new(def, oriented);
            }
        }

        if (ports.Count < 1)
        {
            throw new InvalidOperationException($"{Name}: No ports defined");
        }

        Ports = ports.ToFrozenDictionary();
    }

    public void AddFluid(string id, float amount)
    {
        if (amount <= 0 || id is null || id.Length == 0)
        {
            return;
        }

        if (IsContaminated)
        {
            SetWaterHeight(FluidHeight + amount);
            return;
        }

        if (NetworkGoodId is { } have && have != id)
        {
            SetWaterHeight(FluidHeight + amount);
            Graph?.Contaminate(new(have, id));
            return;
        }

        Graph?.AdoptFluid(id);
        FluidGoodId = Graph?.FluidGoodId ?? id;
        SetWaterHeight(FluidHeight + amount);
    }

    public void RemoveFluid(float amount) => SetWaterHeight(FluidHeight - amount);

    internal void SetVolume(float volume) => SetWaterHeight(volume);

    internal void AssignFluidId(string? id)
    {
        if (id is null || id.Length == 0 || IsContaminated)
        {
            return;
        }

        Graph?.AdoptFluid(id);
        if (Graph is { FluidGoodId: { } network })
        {
            FluidGoodId = network;
            return;
        }

        FluidGoodId ??= id;
    }

    internal void SyncNetworkGood()
    {
        if (IsContaminated || Graph is not { FluidGoodId: { } id })
        {
            return;
        }

        FluidGoodId = id;
    }

    internal void MarkContaminated(PipeContaminationCause cause)
    {
        FluidGoodId = FluidGoodIdContaminated;
        if (cause.HasPair)
        {
            ContaminationCause = cause;
        }
    }

    internal void ClearFluid()
    {
        FluidHeight = 0;
        FluidGoodId = null;
        ContaminationCause = default;
    }

    void SetWaterHeight(float height) => FluidHeight = Math.Clamp(height, 0, MaxWaterHeight);

    public void Save(IEntitySaver entitySaver)
    {
        var s = entitySaver.GetComponent(SaveKey);
        s.Set(WaterHeightKey, FluidHeight);
        s.Set(FluidGoodIdKey, NetworkGoodId ?? FluidGoodId ?? "");
        s.Set(MixGoodAKey, ContaminationCause.GoodA ?? "");
        s.Set(MixGoodBKey, ContaminationCause.GoodB ?? "");
    }

    public void Load(IEntityLoader entityLoader)
    {
        if (!entityLoader.TryGetComponent(SaveKey, out var s)) { return; }
        FluidHeight = s.Get(WaterHeightKey);

        var id = s.Get(FluidGoodIdKey);
        FluidGoodId = id is null || id.Length == 0 ? null : id;
        ContaminationCause = new(ReadOptional(s, MixGoodAKey), ReadOptional(s, MixGoodBKey));
    }

    static string? ReadOptional(IObjectLoader s, PropertyKey<string> key)
    {
        if (!s.Has(key))
        {
            return null;
        }

        var value = s.Get(key);
        return value is null || value.Length == 0 ? null : value;
    }

    public void OnEnterFinishedState()
    {
        if (Spec is null || Ports is null)
        {
            return;
        }

        registry.Register(this);
    }

    public void OnExitFinishedState()
    {
        if (Spec is null || Ports is null)
        {
            return;
        }

        registry.Unregister(this);
    }
}
