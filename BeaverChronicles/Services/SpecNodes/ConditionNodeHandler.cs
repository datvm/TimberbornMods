namespace BeaverChronicles.Services.SpecNodes;

[MultiBind(typeof(ISpecNodeHandler))]
public class ConditionNodeHandler : ISpecNodeHandler
{
    public const string NodeType = "Condition";
    public string ForType => NodeType;

    public void HandleNode(ChronicleEventNodeSpec node, SpecChronicleEventHelper helper)
    {
        throw new NotImplementedException();
    }

    public void RestoreGameState(ChronicleEventNodeSpec node, SpecChronicleEventHelper helper)
    {
        throw new NotSupportedException("The game should not be saved during a condition node");
    }
}
