namespace ModdableTimberborn.Areas;

public readonly record struct CharacterTrackedPosition(Vector3 Position, Vector3Int Cell, int Segment);
public readonly record struct CharacterTrackedPositionChange(CharacterTrackedPosition Old, CharacterTrackedPosition New);

public class CharacterPositionTracker(AreaSegmentService segmentService) : TickableComponent, IAwakableComponent
{
    static readonly Vector3Int Default = new(-1, -1, -1);

    public Character Character { get; private set; } = null!;

    public CharacterType CharacterType { get; private set; }
    public Vector3Int Cell { get; private set; } = Default;
    public int Segment { get; private set; } = -1;
    public Vector3 Position => transform.position;
    Vector3 prevPosition = Default;

    public event EventHandler<CharacterTrackedPositionChange>? OnPositionChanged;
    public event EventHandler<CharacterTrackedPositionChange>? OnCellChanged;

    Transform transform = null!;

    public void Awake()
    {
        Character = GetComponent<Character>();
        CharacterType = this.GetCharacterType();
        transform = Transform;
    }

    public override void Tick()
    {
        var pos = transform.position;
        if (pos == prevPosition) { return; }

        var oldPos = prevPosition;
        prevPosition = pos;

        var oldCell = Cell;
        var oldSegment = Segment;
        var currInt = pos.FloorToInt();
        var cellChanged = currInt != oldCell;
        if (cellChanged)
        {
            Cell = currInt;
            Segment = segmentService.GetSegment(currInt);
        }

        if (OnPositionChanged is null && (!cellChanged || OnCellChanged is null)) { return; }

        var evArgs = new CharacterTrackedPositionChange(
            new CharacterTrackedPosition(oldPos, oldCell, oldSegment),
            new CharacterTrackedPosition(pos, Cell, Segment)
        );

        OnPositionChanged?.Invoke(this, evArgs);
        if (cellChanged)
        {
            OnCellChanged?.Invoke(this, evArgs);
        }
    }

}

