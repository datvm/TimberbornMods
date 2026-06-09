namespace BeaverChronicles.Services.SpecNodes;

public interface ISpecNodeHandler
{
    string ForType { get; }

    void HandleNode(ChronicleEventNodeSpec node, SpecChronicleEventController controller);
    void RestoreGameState(ChronicleEventNodeSpec node, SpecChronicleEventController controller);
}
