namespace BeaverChronicles.Services.SpecNodes;

[MultiBind(typeof(ISpecNodeHandler))]
public class CameraHandler(
    CameraService cameraService,
    CameraShakeService cameraShakeService,
    EntitySelectionService entitySelectionService
) : NodeHandlerBase<CameraData>
{
    public override string ForType => "Camera";

    protected override string? InternalHandleNode(CameraData data, ChronicleEventNodeSpec node, SpecChronicleEventController controller)
    {
        node.LogVerbose(() => $"Camera node requested: {data}");

        if (!FocusOnEntity())
        {
            if (data.PositionValue is Vector3 pos)
            {
                cameraService.MoveTargetTo(pos);
            }

            if (data.VerticalAngle.HasValue)
            {
                cameraService.VerticalAngle = data.VerticalAngle.Value;
            }

            if (data.HorizontalAngle.HasValue)
            {
                cameraService.HorizontalAngle = data.HorizontalAngle.Value;
            }

            if (data.ZoomLevel.HasValue)
            {
                cameraService.ZoomLevel = data.ZoomLevel.Value;
            }
        }

        if (data.CameraShake is { } cs)
        {
            cameraShakeService.Shake(cs.Duration, cs.Strength);
        }

        return node.NextNodeId;

        bool FocusOnEntity()
        {
            if (data.FocusOnEntityIds.Length == 0) { return false; }

            var entity = controller.GetEntities(data.FocusOnEntityIds).FirstOrDefault();
            if (entity is null) { return false; }

            entitySelectionService.SelectAndFocusOn(entity);
            return true;
        }
    }

}
