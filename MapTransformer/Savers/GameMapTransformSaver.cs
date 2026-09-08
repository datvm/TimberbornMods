namespace MapTransformer.Savers;

[BindSingleton(Contexts = BindAttributeContext.Game, As = typeof(IMapTransformSaver))]
class GameMapTransformSaver(
    GameSaver gameSaver,
    ValidatingGameLoader validatingGameLoader,
    SettlementReferenceService settlementRefService
) : IMapTransformSaver
{
    public void Load(IMapTransformSaveReference saveReference)
    {
        if (saveReference is not GameSaveRef wrapper)
        {
            throw new ArgumentException("Invalid save reference type.", nameof(saveReference));
        }

        validatingGameLoader.LoadGame(wrapper.SaveReference);
    }

    public async Task<IMapTransformSaveReference> SaveAsync()
    {
        TaskCompletionSource<bool> tcs = new();
        var saveRef = new SaveReference("MapResized", settlementRefService.SettlementReference);
        gameSaver.QueueSaveSkippingNameValidation(saveRef, () => tcs.TrySetResult(true));
        await tcs.Task;
        return new GameSaveRef(saveRef);
    }

    readonly record struct GameSaveRef(SaveReference SaveReference) : IMapTransformSaveReference;
}
