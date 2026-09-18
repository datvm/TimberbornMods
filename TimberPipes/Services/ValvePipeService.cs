namespace TimberPipes.Services;

[BindSingleton]
public class ValvePipeService(IBlockService blockService, IGoodService goods, EventBus eventBus) : ILoadableSingleton
{
    public readonly IGoodService Goods = goods;

    readonly List<ExtractionPipe> extractions = [];
    HashSet<string>? liquidIds;

    public HashSet<string> LiquidIds => liquidIds ??= CollectLiquidIds(Goods);

    public void Load() => eventBus.Register(this);

    public void Track(ExtractionPipe extraction)
    {
        if (!extractions.Contains(extraction))
        {
            extractions.Add(extraction);
        }
    }

    public void Untrack(ExtractionPipe extraction) => extractions.Remove(extraction);

    [OnEvent]
    public void OnEnteredUnfinishedState(EnteredUnfinishedStateEvent _) => InvalidateIoTargets();

    [OnEvent]
    public void OnExitedUnfinishedState(ExitedUnfinishedStateEvent _) => InvalidateIoTargets();

    [OnEvent]
    public void OnEnteredFinishedState(EnteredFinishedStateEvent _) => InvalidateIoTargets();

    [OnEvent]
    public void OnExitedFinishedState(ExitedFinishedStateEvent _) => InvalidateIoTargets();

    [OnEvent]
    public void OnEntityDeleted(EntityDeletedEvent _) => InvalidateIoTargets();

    public void InvalidateIoTargets()
    {
        foreach (var extraction in extractions)
        {
            extraction.InvalidateIoTargets();
        }
    }

    static HashSet<string> CollectLiquidIds(IGoodService goods)
    {
        HashSet<string> ids = [];
        foreach (var id in goods.GetGoodsForType(PipeFluids.LiquidGoodType))
        {
            if (goods.HasGood(id))
            {
                ids.Add(id);
            }
        }

        return ids;
    }

    public IBuildingPipeConnection? FindConnection(BuildingPipe pipe, PipePortState required, bool give)
    {
        if (pipe.Ports is not { } ports)
        {
            return null;
        }

        foreach (var port in ports.Values)
        {
            if ((port.PortSpec.State & required) == 0)
            {
                continue;
            }

            var approach = port.GetOppositePortDefinition();
            if (ConnectionAt(pipe, approach, give) is { } connection)
            {
                return connection;
            }
        }

        return null;
    }

    public bool FacesVisualBuilding(BuildingPipe pipe, Vector3Int coordinates, Direction3D outward, bool give)
    {
        var approach = new PipePortDefinition(coordinates + outward.ToOffset(), outward.Across());
        return ConnectionAt(pipe, approach, give) is not null;
    }

    IBuildingPipeConnection? ConnectionAt(BuildingPipe pipe, PipePortDefinition approach, bool give)
    {
        foreach (var obj in blockService.GetObjectsAt(approach.Coordinates))
        {
            if (obj.Overridable)
            {
                continue;
            }

            var target = obj.GetComponent<BuildingPipeTarget>();
            if (!target)
            {
                continue;
            }

            if (target.TryConnecting(pipe, approach, give) is { } connection)
            {
                return connection;
            }
        }

        return null;
    }

    public List<string> ExtractDropdownGoods(IBuildingPipeConnection? connection, string? storedGoodId)
    {
        if (connection is null)
        {
            return [];
        }

        List<string> known = [.. connection.GetLiquidIds()];
        return ValvePipeIo.ExtractDropdownGoods(known, LiquidIds, storedGoodId);
    }
}
