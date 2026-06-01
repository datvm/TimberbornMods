namespace BeaverChronicles.Services.SpecNodes;

public interface ISpecNodeHandler
{
    ChronicleEventNodeType ForType { get; }

    void HandleNode(ChronicleEventNodeSpec node, SpecChronicleEventHelper helper);
    void RestoreGameState(ChronicleEventNodeSpec node, SpecChronicleEventHelper helper);
}
