namespace Crane.Components;

public interface ICraneRubbleProcessor
{
    bool TryProcessRubble(CraneComponent crane, RecoveredGoodStack stack, int items);
}
