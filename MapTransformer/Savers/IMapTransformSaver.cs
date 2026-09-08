namespace MapTransformer.Savers;

interface IMapTransformSaver
{
    Task<IMapTransformSaveReference> SaveAsync();
    void Load(IMapTransformSaveReference saveReference);
}

interface IMapTransformSaveReference;
