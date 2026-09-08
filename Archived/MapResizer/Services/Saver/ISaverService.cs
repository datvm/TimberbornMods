namespace MapResizer.Services.Saver;

public interface ISaverService
{
    Task<ISaveReference> SaveAsync();
    void Load(ISaveReference saveReference);
}

public interface ISaveReference
{
}