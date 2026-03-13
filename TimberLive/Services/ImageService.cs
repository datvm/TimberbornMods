namespace TimberLive.Services;

[SelfService(Lifetime = ServiceLifetime.Singleton)]
public class ImageService(ApiService api)
{
    const string DataUrlPrefix = "data:image/png;base64,";

    static readonly TimeSpan FetchPendingTime = TimeSpan.FromMilliseconds(300);
    readonly ConcurrentDictionary<string, string> imageData = new(StringComparer.OrdinalIgnoreCase);

    readonly SemaphoreSlim fetchLock = new(1, 1);
    Task? fetchWaitingTask;
    readonly HashSet<string> pendingFetches = [];

    public async Task<string> GetImageAsync(string path)
    {
        while (true)
        {
            if (imageData.TryGetValue(path, out var data))
            {
                return data;
            }

            await FetchAsync(path);
        }
    }

    async Task FetchAsync(string path)
    {
        Task? waitingTask;

        await fetchLock.WaitAsync();
        try
        {
            pendingFetches.Add(path);

            waitingTask = fetchWaitingTask;
            waitingTask ??= fetchWaitingTask = Task.Delay(FetchPendingTime);
        }
        finally
        {
            fetchLock.Release();
        }

        await waitingTask;

        await fetchLock.WaitAsync();
        try
        {
            if (waitingTask != fetchWaitingTask) { return; }
            fetchWaitingTask = null;

            await PerformFetchAsync();
        }
        finally
        {
            fetchLock.Release();
        }
    }

    async Task PerformFetchAsync()
    {
        var paths = pendingFetches
            .Where(p => !imageData.ContainsKey(p))
            .ToArray();
        pendingFetches.Clear();

        var body = string.Join(';', paths);
        using var req = new HttpRequestMessage(HttpMethod.Post, "file/images/");
        req.Content = new StringContent(body);

        var images = await api.SendAsync<string[]>(req);
        for (int i = 0; i < paths.Length; i++)
        {
            imageData[paths[i]] = DataUrlPrefix + images[i];
        }
    }

}
