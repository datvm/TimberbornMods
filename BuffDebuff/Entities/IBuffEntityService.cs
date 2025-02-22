namespace BuffDebuff;

public interface IBuffEntityService
{
    IBuffEntity Get(long id);
    T Get<T>(long id) where T : IBuffEntity;
    void Load();
    long NewUuid();
    void Register<T>(T entity) where T : IBuffEntity;
    void Save(ISingletonSaver singletonSaver);
    void Unregister<T>(T entity) where T : IBuffEntity;
}