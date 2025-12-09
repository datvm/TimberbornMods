namespace BuffDebuff;

public interface IBuffableService
{
    BuffableComponent this[long index] { get; }

    ICollection<BuffableComponent> Buffables { get; }

    void Load();
    void Register(BuffableComponent c);
    void Unregister(BuffableComponent c);
}