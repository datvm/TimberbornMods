namespace DistroStorage.Components;

public interface IDistroReceiver : IDistroComponent, IPrioritizable
{
    string? CanReceiveGood(HashSet<string> goodIds);

    void TransferIn(GoodAmount good);
}
