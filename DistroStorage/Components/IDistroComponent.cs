namespace DistroStorage.Components;

public interface IDistroComponent
{
    bool IsSender { get; }
    bool IsReceiver { get; }

    bool DisabledBySetting { get; }

    bool Enabled { get; }
    bool Active { get; }

    BoundsInt Bounds { get; }
    IEnumerable<GoodAmount> Goods { get; }
    IEnumerable<string> GoodIds { get; }

    IReadOnlyCollection<IDistroComponent> Connections { get; }
    bool ActiveAndEnabled { get; }

    void ConnectWith(IDistroComponent component);
    void DisconnectFrom(IDistroComponent component);
    void ClearConnections();

    void SetEnabled(bool enabled);
}
