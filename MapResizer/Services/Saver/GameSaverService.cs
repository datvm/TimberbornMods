namespace MapResizer.Services.Saver;

public class GameSaverService(
    GameSaver gameSaver,
    ValidatingGameLoader validatingGameLoader,
    SettlementNameService settlementNameService
) : ISaverService
{
    public void Load(ISaveReference saveReference)
    {
        if (saveReference is not SaveReferenceWrapper wrapper)
        {
            throw new ArgumentException("Invalid save reference type.", nameof(saveReference));
        }

        validatingGameLoader.LoadGame(wrapper.SaveReference);
    }

    public async Task<ISaveReference> SaveAsync()
    {
        TaskCompletionSource<bool> tcs = new();

        var saveRef = new SaveReference(settlementNameService.SettlementName, "MapResized");

        gameSaver.QueueSave(
            saveRef,
            () =>
            {
                Debug.Log("Resized map saved.");
                tcs.TrySetResult(true);
            });

        await tcs.Task;

        return new SaveReferenceWrapper(saveRef);
    }

    class SaveReferenceWrapper(SaveReference saveReference) : ISaveReference
    {
        public SaveReference SaveReference { get; } = saveReference;
    }

}