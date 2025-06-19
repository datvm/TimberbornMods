namespace MacroManagement.Components.DummyComponents;

public class DummyCameraTarget(Vector3 position) : ICameraTarget
{
    public Vector3 CameraTargetPosition { get; } = position;
}
