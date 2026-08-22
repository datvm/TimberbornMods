namespace Crane.Components;

public interface ICraneRangeModifier
{
    int RangeDelta { get; }
    event EventHandler? OnRangeChanged;
}
