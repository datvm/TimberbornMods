namespace WeatherEditor.Services;

public class GameSaverService(
    GameSaver gameSaver,
    ValidatingGameLoader validatingGameLoader,
    SettlementNameService settlementNameService
)
{

    public async Task<SaveReference> SaveAsync()
    {
        TaskCompletionSource<bool> tcs = new();

        var saveRef = new SaveReference(settlementNameService.SettlementName, "WeatherEdited");

        gameSaver.QueueSave(
            saveRef,
            () =>
            {
                Debug.Log("Weather edited saved.");
                tcs.TrySetResult(true);
            });

        await tcs.Task;

        return saveRef;
    }

    public void Load(SaveReference r)
    {
        validatingGameLoader.LoadGame(r);
    }

}
