namespace BeaverChronicles.Services.SpecNodes;

[MultiBind(typeof(ISpecNodeHandler))]
public class SpawnObjectHandler(
    BlockObjectSpawningHelper helper
) : NodeHandlerBase<SpawnObjectData>
{
    public override string ForType => "SpawnObject";

    protected override string? InternalHandleNode(SpawnObjectData data, ChronicleEventNodeSpec node, SpecChronicleEventController controller)
    {
        if (!helper.TryGetBlockObjectSpec(data.TemplateNames, out var template, true))
        {
            return data.FailedNodeId;
        }

        var x = controller.FormatTextInt(data.X);
        var y = controller.FormatTextInt(data.Y);
        var z = controller.FormatTextInt(data.Z);
        var orientation = controller.FormatTextEnum<Orientation>(data.Orientation);
        var flip = controller.FormatTextBool(data.Flipped);

        var placement = new Placement(new(x, y, z), orientation, flip ? FlipMode.Flipped : FlipMode.Unflipped);
        var conflictMode = controller.FormatTextEnum<SpawnObjectConflictMode>(data.ConflictMode);

        bool successful = false;
        switch (conflictMode)
        {
            case SpawnObjectConflictMode.Ignore:
                successful = helper.IsPlacementValid(template, placement);
                if (successful)
                {
                    helper.PlaceObject(template, placement);
                }
                break;
            case SpawnObjectConflictMode.Destructive:
                successful = helper.TryPlacingWithDestruction(template, placement).Result;
                break;
            default:
                throw new InvalidOperationException($"Unsupported conflict mode: {conflictMode}");
        }

        return successful ? node.NextNodeId : data.FailedNodeId;
    }


}
