namespace BeaverChronicles.Services.SpecNodes;

[MultiBind(typeof(ISpecNodeHandler))]
public class SpawnObjectHandler(
    TemplateNameMapper templateMapper
) : NodeHandlerBase<SpawnObjectData>
{
    public override string ForType => "SpawnObject";

    protected override string? InternalHandleNode(SpawnObjectData data, ChronicleEventNodeSpec node, SpecChronicleEventController controller)
    {
        if (data.TemplateNames.Length == 0)
        {
            throw new ArgumentException("SpawnObject node must have at least one template name.");
        }

        var template = GetTemplate(data);
        if (template is null) { return data.FailedNodeId; }

        

        return node.NextNodeId;
    }

    PlaceableBlockObjectSpec? GetTemplate(SpawnObjectData data)
    {
        foreach (var n in data.TemplateNames)
        {
            if (templateMapper.TryGetTemplate(n, out var spec))
            {
                return spec.GetSpec<PlaceableBlockObjectSpec>()
                    ?? throw new InvalidDataException($"Template '{n}' is not a placeable block object.");
            }
        }

        return null;
    }

}
