namespace PowerLines.Components;

[AddTemplateModule2(typeof(MechanicalNode))]
public class PowerLineComponent(PowerLinesService services) : BaseComponent, IAwakableComponent, IInitializableEntity, IDeletableEntity, IEntityDescriber
{

    BlockObject bo = null!;
    EntityComponent entity = null!;
    bool isGenerator;
    bool initializedNumbers = false;

    public ImmutableArray<Vector3> ConnectionLocations { get; private set; }
    public int MaxConnections { get; private set; }
    public float MaxConnectionLength { get; private set; }

    bool defaultMaxConn = true, defaultMaxConnLength = true;

    public bool IsFinished => bo.IsFinished;
    public bool IsDeleted => entity.Deleted;
    public Guid EntityId => entity.EntityId;

    public void Awake()
    {
        bo = GetComponent<BlockObject>();
        entity = GetComponent<EntityComponent>();

        isGenerator = (GetComponent<MechanicalNodeSpec>()?.PowerOutput ?? 0) > 0;

        // Need to run these first to get the description for the entity
        if (isGenerator)
        {
            defaultMaxConn = false;
            defaultMaxConnLength = false;
        }
        else
        {
            var spec = GetComponent<PowerLineSpec>();
            if (spec is not null)
            {
                defaultMaxConnLength = spec.MaxConnectionLength == null;
                defaultMaxConn = spec.MaxConnections == null;
            }
        }
    }

    public void InitializeEntity()
    {
        InitializeNumbers();

        var spec = GetComponent<PowerLineSpec>();

        var connectionLocations = spec?.ConnectionLocations ?? [];
        if (connectionLocations.Length > 0)
        {
            ConnectionLocations = [.. connectionLocations.Select(bo.TransformCoordinates)];
        }
        else
        {
            var occupiedTop = bo.PositionedBlocks.GetOccupiedBlocks().Max(b => b.Coordinates.z);
            ConnectionLocations = [GetComponent<BlockObjectCenter>().GridCenter with { z = occupiedTop + .98f }];
        }
    }

    void InitializeNumbers()
    {
        if (initializedNumbers) { return; }
        initializedNumbers = true;

        var spec = GetComponent<PowerLineSpec>();
        MaxConnections = spec?.MaxConnections ?? (isGenerator ? services.DefaultMaxGeneratorConnections : services.DefaultMaxConnections);
        MaxConnectionLength = spec?.MaxConnectionLength ?? (isGenerator ? services.DefaultMaxGeneratorConnectionLength : services.DefaultMaxConnectionLength);
    }

    public void DeleteEntity() => services.OnPowerLineDeleted(this);

    public float ShortestDistanceTo(PowerLineComponent other)
        => GetClosestEndpoints(ConnectionLocations, other.ConnectionLocations).Distance;

    public (Vector3 From, Vector3 To, float Distance) GetClosestEndpoints(PowerLineComponent other)
        => GetClosestEndpoints(ConnectionLocations, other.ConnectionLocations);

    public static (Vector3 From, Vector3 To, float Distance) GetClosestEndpoints(
        ImmutableArray<Vector3> fromLocations,
        ImmutableArray<Vector3> toLocations)
    {
        var bestDistance = float.MaxValue;
        var bestFrom = fromLocations[0];
        var bestTo = toLocations[0];

        foreach (var from in fromLocations)
        {
            foreach (var to in toLocations)
            {
                var distance = Vector3.Distance(from, to);
                if (distance < bestDistance)
                {
                    bestDistance = distance;
                    bestFrom = from;
                    bestTo = to;
                }
            }
        }

        return (bestFrom, bestTo, bestDistance);
    }

    public IEnumerable<EntityDescription> DescribeEntity()
    {
        if (defaultMaxConn && defaultMaxConnLength) { return []; }

        InitializeNumbers();

        var str = "";
        var t = services.t;
        if (!defaultMaxConn)
        {
            str += SpecialStrings.RowStarter + t.T("LV.PL.PowerLinesCount", MaxConnections) + Environment.NewLine;
        }

        if (!defaultMaxConnLength)
        {
            str += SpecialStrings.RowStarter + t.T("LV.PL.MaxDistance", MaxConnectionLength) + Environment.NewLine;
        }

        return [EntityDescription.CreateTextSection(str.Trim(), 1200)];
    }

}
