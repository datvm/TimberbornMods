namespace RadialToolbar.Models;

readonly record struct RayHit(Vector2 Point, Direction Side);

public readonly record struct ToolbarRay(
    Vector2 Start,
    Vector2 End,
    Vector2 Direction,
    float AngleDeg,
    Direction Side
);

public readonly record struct ToolbarSegment(
    int Index,
    Vector2 Center,
    float StartAngleDeg,
    float EndAngleDeg,
    ToolbarRay StartBoundary,
    ToolbarRay EndBoundary,
    Direction Direction
)
{
    public float MidAngleDeg => (StartAngleDeg + EndAngleDeg) * 0.5f;
}

[Flags]
public enum Direction
{
    None = 0,
    Up = 1,
    Right = 2,
    Down = 4,
    Left = 8,
}