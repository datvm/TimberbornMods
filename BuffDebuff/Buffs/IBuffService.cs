
namespace BuffDebuff;

public interface IBuffService
{
    IBuff this[long id] { get; }

    IEnumerable<IBuff> Buffs { get; }

    void Apply<T>(T instance) where T : BuffInstance;
    void Remove<T>(T instance) where T : BuffInstance;
    void SetActive<T>(T instance, bool active) where T : BuffInstance;
    void Activate<T>(T instance) where T : BuffInstance;
    void Deactivate<T>(T instance) where T : BuffInstance;

    void Load();
    void Save(ISingletonSaver singletonSaver);

}