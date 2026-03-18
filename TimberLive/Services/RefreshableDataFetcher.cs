namespace TimberLive.Services;

public enum FetcherState
{
    Ready,
    Running,
    Fetching,
    Paused,
    Disposed
}

public class RefreshableDataFetcher(TimeSpan refreshTime, Func<Task> refreshTask) : IDisposable
{
    public TimeSpan RefreshTime { get; } = refreshTime;

    public DateTime LastRefreshTime { get; private set; }
    public FetcherState State { get; private set; } = FetcherState.Ready;
    public event Action? StateChanged;

    public bool IsRunning => !once && (State == FetcherState.Running || State == FetcherState.Fetching);

    bool once;
    CancellationTokenSource? cts;
    CancellationTokenSource? delayCts;

    public void RefreshNow() => delayCts?.Cancel();

    void SetState(FetcherState newState)
    {
        if (State == newState) { return; }

        State = newState;
        StateChanged?.Invoke();
    }

    public void RefreshOnce()
    {
        once = true;
        Start();
    }

    public void Start()
    {
        if (this.cts?.IsCancellationRequested == false) { return; }

        var cts = this.cts = new();
        var ct = cts.Token;

        Task.Run(async () =>
        {
            try
            {
                while (!ct.IsCancellationRequested)
                {
                    SetState(FetcherState.Fetching);
                    await refreshTask().ConfigureAwait(false);
                    LastRefreshTime = DateTime.Now;

                    if (once)
                    {
                        once = false;
                        cts.Cancel();
                        break;
                    }
                    
                    SetState(FetcherState.Running);

                    try
                    {
                        delayCts = new();
                        using var linked = CancellationTokenSource.CreateLinkedTokenSource(ct, delayCts.Token);
                        await Task.Delay(RefreshTime, linked.Token).ConfigureAwait(false);
                    }
                    catch (OperationCanceledException) { }
                }
            }
            catch (OperationCanceledException) { }

            SetState(FetcherState.Paused);
        }, ct);
    }

    public void Pause()
    {
        if (cts is null) { return; }

        cts.Cancel();
    }

    public static RefreshableDataFetcher Create(Func<Task> refreshTask, StorageService storageService, bool start = true)
    {
        var refreshTime = TimeSpan.FromSeconds(storageService.GetValue<int>(StorageKey.RefreshTime));
        var fetcher = new RefreshableDataFetcher(refreshTime, refreshTask);
        if (start)
        {
            fetcher.Start();
        }
        return fetcher;
    }
    public void Dispose()
    {
        GC.SuppressFinalize(this);
        Pause();
        cts?.Dispose();
        SetState(FetcherState.Disposed);
    }
}
