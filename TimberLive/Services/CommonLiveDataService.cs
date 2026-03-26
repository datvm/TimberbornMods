namespace TimberLive.Services;

public class CommonLiveDataService(ApiService api, StorageService storageService) : IApiConnectionListener
{

    public HttpCommonData? Data { get; private set; }
    RefreshableDataFetcher? fetcher;

    public event EventHandler<HttpCommonData>? NewDataArrived;

    public async Task OnConnectedAsync()
    {
        await OnDisconnectedAsync();

        RefreshableDataFetcher fetcher = null!;
        fetcher = this.fetcher = RefreshableDataFetcher.Create(async () =>
        {
            await LoadDataAsync(fetcher);
        }, storageService);
    }

    public void UpdateNow() => fetcher?.RefreshNow();

    async Task LoadDataAsync(RefreshableDataFetcher fetcher)
    {
        if (IsDisposed()) { return; }

        var data = await api.GetAsync<HttpCommonData>("live-data");
        if (IsDisposed()) { return; }

        // In WASM, this almost guarantees there is no race condition
        Data = data;
        NewDataArrived?.Invoke(this, data);

        bool IsDisposed() => this.fetcher != fetcher || !fetcher.IsRunning;
    }

    public async Task OnDisconnectedAsync()
    {
        if (fetcher is not null)
        {
            fetcher?.Dispose();
            fetcher = null;
        }

        Data = null;
    }
}
