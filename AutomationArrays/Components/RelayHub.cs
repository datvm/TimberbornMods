namespace AutomationArrays.Components;

[AddTemplateModule2(typeof(RelayHubSpec))]
public class RelayHub() : BaseComponent, IAwakableComponent, IArrayTransmitter, ICombinationalTransmitter, IPersistentEntity
{
    static readonly ComponentKey SaveKey = new(nameof(RelayHub));
    static readonly ListKey<Automator> InputsKey = new("Inputs");
    static readonly ListKey<int> SingleParametersKey = new("SingleParameters");
    static readonly ListKey<int> ArrayParametersKey = new("ArrayParameters");
    static readonly PropertyKey<bool> SingleModeNotKey = new("SingleModeNot");
    static readonly PropertyKey<bool> ArrayModeNotKey = new("ArrayModeNot");

#nullable disable
    SegmentedIlluminator illuminator;
    Automator automator;
#nullable enable

    public RelayHubSingleMode SingleMode { get; private set; }
    public bool SingleModeNot { get; private set; }
    public RelayHubArrayMode ArrayMode { get; private set; }
    public bool ArrayModeNot { get; private set; }
    public int[] SingleParameters { get; private set; } = [];
    public int[] ArrayParameters { get; private set; } = [];

    readonly List<AutomatorConnection> connections = [];
    readonly List<bool> states = [];

    public IEnumerable<AutomatorConnection> Connections => connections;
    public IReadOnlyList<bool> States => states;

    public void Awake()
    {
        illuminator = GetComponent<SegmentedIlluminator>();
        automator = GetComponent<Automator>();
    }

    public void Add()
    {
        var connection = automator.AddInput();
        connections.Add(connection);

        Evaluate();
    }

    public void Remove(int index)
    {
        var conn = connections[index];
        connections.RemoveAt(index);
        conn.Remove();

        Evaluate();
    }

    readonly List<bool> inputCache = [];
    public void Evaluate()
    {
        var newState = RelayHubEvaluator.Evaluate(this, inputCache, states);
        automator.SetState(newState);
    }

    public void Load(IEntityLoader entityLoader)
    {

    }

    public void Save(IEntitySaver entitySaver)
    {

    }
}
