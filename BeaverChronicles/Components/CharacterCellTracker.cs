namespace BeaverChronicles.Components;

[AddTemplateModule2(typeof(Character))]
public class CharacterCellTracker(AreaSegmentService segmentService) : TickableComponent, IAwakableComponent
{
    static readonly Vector3Int Default = new(-1, -1, -1);

    public CharacterType CharacterType { get; private set; }
    public Vector3Int Cell { get; private set; } = Default;
    public int Segment { get; private set; } = -1;
    public Vector3 Position => transform.position;

    public event Action<Vector3Int>? OnCellChanged;

    Transform transform = null!;

    public void Awake()
    {
        CharacterType = this.GetCharacterType();
        transform = Transform;
    }

    public override void Tick()
    {
        var currInt = Position.FloorToInt();
        if (currInt != Cell)
        {
            Cell = currInt;
            Segment = segmentService.GetSegment(currInt);
            OnCellChanged?.Invoke(currInt);
        }
    }

}
