namespace DistroStorage.Components;

public interface IDistroReceiver : IDistroComponent, IPrioritizable
{
    string? CanReceiveGood(HashSet<string> goodIds);
    void Deserialize(DistroReceiverSerializableModel model);
    DistroReceiverSerializableModel Serialize();
    void TransferIn(GoodAmount good);
}
