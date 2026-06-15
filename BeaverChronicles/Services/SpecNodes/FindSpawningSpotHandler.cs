namespace BeaverChronicles.Services.SpecNodes;

[MultiBind(typeof(ISpecNodeHandler))]
public class FindSpawningSpotHandler(
    BlockObjectSpawningHelper helper
) : NodeHandlerBase<FindSpawningSpotData>
{
    public override string ForType => "FindSpawningSpot";

    protected override string? InternalHandleNode(FindSpawningSpotData data, ChronicleEventNodeSpec node, SpecChronicleEventController controller)
    {
        if (!helper.TryGetBlockObjectSpec(data.TemplateNames, out var spec, true))
        {
            node.LogVerbose(() => $"No matching template found. Going to {data.FailedNodeId}");
            return data.FailedNodeId;
        }

        var prefix = data.ResultName;
        if (string.IsNullOrEmpty(prefix))
        {
            throw new InvalidOperationException($"FindSpawningSpot node {node.Id} has empty ResultName.");
        }

        if (!helper.TryFindPlacement(spec, out var placement, new(data.NearBuildingRadius, data.LimitArea)))
        {
            node.LogVerbose(() => $"No valid spawning spot found. Going to {data.FailedNodeId}");
            return data.FailedNodeId;
        }

        var p = placement.Value;
        var (x, y, z) = p.Coordinates;

        var customParameters = controller.CurrentRecord.CustomParameters;
        customParameters[$"{prefix}_X"] = x.ToString();
        customParameters[$"{prefix}_Y"] = y.ToString();
        customParameters[$"{prefix}_Z"] = z.ToString();
        customParameters[$"{prefix}_Orientation"] = p.Orientation.ToString();
        node.LogVerbose(() => $"Found spawning spot at ({x}, {y}, {z}), orientation {p.Orientation}. Going to {node.NextNodeId}");

        return node.NextNodeId;
    }

}
