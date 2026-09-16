namespace TimberPipes.Services;

[BindSingleton]
public class ValvePipeService(IBlockService blockService, IGoodService goods)
{
    public readonly IGoodService Goods = goods;

    HashSet<string>? liquidIds;

    public HashSet<string> LiquidIds => liquidIds ??= CollectLiquidIds(Goods);

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
