namespace SyncableSettings.Services;

public class PlayerPrefTimer(Action saveAction, int delayMs = 200)
{

    bool pending = false;
    Timer? timer;
    readonly object _lock = new();

    public void MarkDirty()
    {
        pending = true;
        timer?.Dispose();
        timer = new Timer(_ =>
        {
            Save();
        }, null, delayMs, Timeout.Infinite);
    }

    private void Save()
    {
        lock (_lock)
        {
            if (!pending) { return; }
            pending = false;            
        }

        saveAction();
    }

}
