namespace BeaverChronicles.Events;

public interface IAreaChronicleEvent : IChronicleEvent
{
    SegmentedArea[] Areas { get; }
    CharacterType CharacterType { get; }
    bool AreasActive { get; }
}

public readonly record struct SegmentedArea(BoundsInt Bounds, int[] Segments);