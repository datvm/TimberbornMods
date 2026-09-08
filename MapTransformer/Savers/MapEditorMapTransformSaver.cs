namespace MapTransformer.Savers;

[BindSingleton(Contexts = BindAttributeContext.MapEditor, As = typeof(IMapTransformSaver))]
class MapEditorMapTransformSaver(
    MapEditorSceneLoader mapEditorSceneLoader,
    MapSaver mapSaver
) : IMapTransformSaver
{
    public void Load(IMapTransformSaveReference saveReference)
    {
        if (saveReference is not MapSaveRef saveRef)
        {
            throw new ArgumentException("Invalid save reference type.", nameof(saveReference));
        }

        mapEditorSceneLoader.LoadMap(saveRef.MapFileReference);
    }

    public async Task<IMapTransformSaveReference> SaveAsync()
    {
        var saveRef = MapFileReference.FromUserFolder("ResizedMap");
        mapSaver.Save(saveRef);
        await Task.CompletedTask;
        return new MapSaveRef(saveRef);
    }

    readonly record struct MapSaveRef(MapFileReference MapFileReference) : IMapTransformSaveReference;
}
