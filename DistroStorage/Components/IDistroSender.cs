namespace DistroStorage.Components;

public interface IDistroSender : IDistroComponent, IPersistentEntity
{
    IDistroSenderTimer Timer { get; }
    void TransferOut(GoodAmount good);

    IEnumerable<IDistroReceiver> GetPrioritizedReceivers();

    float NextTransferTime => Timer.NextTransfer;
}

public interface IDistroSenderTimer
{
    float NextTransfer { get; set; }
    float BaseHoursPerItem { get; }
    float EffectiveHoursPerItem { get; }

    void Reset();

    void SetModifier(string id, float multiplier);
    void RemoveModifier(string id);
    void Load(IObjectLoader s);
    void Save(IObjectSaver s);
}