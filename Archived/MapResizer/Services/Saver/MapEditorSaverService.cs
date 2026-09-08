namespace MapResizer.Services.Saver;

public class MapEditorSaverService(    
    MapEditorSceneLoader mapEditorSceneLoader,
    MapSaver mapSaver
) : ISaverService
{
    public void Load(ISaveReference saveReference)
    {
        if (saveReference is not SaveReferenceWrapper saveRef)
        {
            throw new ArgumentException("Invalid save reference type.", nameof(saveReference));
        }

        mapEditorSceneLoader.LoadMap(saveRef.MapFileReference);
    }

    public async Task<ISaveReference> SaveAsync()
    {
        var saveRef = MapFileReference.FromUserFolder("ResizedMap");
        mapSaver.Save(saveRef);

        await Task.CompletedTask;

        return new SaveReferenceWrapper(saveRef);
    }

    class SaveReferenceWrapper(MapFileReference mapFileReference) : ISaveReference
    {
        public MapFileReference MapFileReference { get; } = mapFileReference;
    }

}
