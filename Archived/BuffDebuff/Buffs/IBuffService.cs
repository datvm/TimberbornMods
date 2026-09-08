
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
    void Register<T>(T b) where T : IBuff;

    TInstance CreateBuffInstance<TBuff, TInstance>(TBuff buff)
        where TBuff : IBuff
        where TInstance : BuffInstance, IBuffInstance<TBuff>, new();
    TInstance CreateBuffInstance<TBuff, TInstance, TValue>(TBuff buff, TValue value)
        where TBuff : IBuff
        where TValue : notnull
        where TInstance : BuffInstance, IBuffInstance<TBuff>, IValuedBuffInstance<TValue>, new();
    IEnumerable<T> GetInstances<T>() where T : BuffInstance;
    void RemoveAllInstances<T>() where T : BuffInstance;
}