namespace MapTransformer.Savers;

[BindSingleton(Contexts = BindAttributeContext.MapEditor, As = typeof(IMapTransformSaver))]
class MapEditorMapTransformSaver(
    MapEditorSceneLoader mapEditorSceneLoader,
    MapSaver mapSaver,
    MapSize mapSize,
    MapMetadataSaveEntryWriter mapMetadataSaveEntryWriter
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
        SyncMetadataSize();
        var saveRef = MapFileReference.FromUserFolder("ResizedMap");
        mapSaver.Save(saveRef);
        await Task.CompletedTask;
        return new MapSaveRef(saveRef);
    }

    readonly record struct MapSaveRef(MapFileReference MapFileReference) : IMapTransformSaveReference;

    void SyncMetadataSize()
    {
        var current = mapMetadataSaveEntryWriter.CurrentMapMetadata;
        if (current is null)
        {
            return;
        }

        var size = mapSize.TerrainSize;
        if (current.Width == size.x && current.Height == size.y)
        {
            return;
        }

        mapMetadataSaveEntryWriter.SetCurrentMapMetadata(new(
            size.x,
            size.y,
            current.MapNameLocKey,
            current.MapDescriptionLocKey,
            current.MapDescription,
            current.IsRecommended,
            current.IsUnconventional,
            current.IsDev));
    }
}
