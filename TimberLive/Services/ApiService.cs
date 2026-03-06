namespace TimberLive.Services;

[SelfService(Lifetime = ServiceLifetime.Singleton)]
public class ApiService : IDisposable
{
    HttpClient http = new();

    bool rechecking;

    public bool Connected { get; private set; }
    public event EventHandler<bool>? ConnectionStateChanged;

    readonly HashSet<Func<Task>> connectionCallbacks = [];

    public Uri CurrentUri { get; private set; } = new("http://localhost:8080");

    public void RegisterConnectedCallback(Func<Task> callback)
    {
        connectionCallbacks.Add(callback);
    }

    public void UnregisterConnectedCallback(Func<Task> callback)
    {
        connectionCallbacks.Remove(callback);
    }

    public async Task<string?> ConnectAsync(string url)
    {
        if (!Uri.TryCreate(url, UriKind.Absolute, out var uri))
        {
            return "Format error";
        }

        http.Dispose();
        http = new()
        {
            BaseAddress = CurrentUri = new(uri, MoreHttpApiUtils.EndpointStart + "/")
        };

        if (!await PingAsync())
        {
            return "Ping failed. Please make sure the API is on.";
        }

        Connected = true;

        foreach (var c in connectionCallbacks)
        {
            await c();
        }

        ConnectionStateChanged?.Invoke(this, true);

        return null;
    }

    async Task RecheckConnectionAsync()
    {
        if (!Connected || rechecking) { return; }

        try
        {
            rechecking = true;
            var ping = await PingAsync();
            if (!ping)
            {
                Disconnect();
            }
        }
        finally
        {
            rechecking = false;
        }
    }

    public void Disconnect()
    {
        Connected = false;
        ConnectionStateChanged?.Invoke(this, false);
    }

    public string GetImageFileUrl(string path) => new Uri(CurrentUri, "file/image?path=" + Uri.EscapeDataString(path)).ToString();

    async Task<bool> PingAsync()
    {
        try
        {
            using var req = new HttpRequestMessage(HttpMethod.Get, "ping");
            await SendAsync(req, true);
            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }

    public async Task<T> GetAsync<T>(string path)
        => await SendAsync<T>(path, HttpMethod.Get);

    public async Task<T> SendAsync<T>(string path, HttpMethod method)
    {
        using var req = new HttpRequestMessage(method, path);
        return await SendAsync<T>(req);
    }

    public async Task<T> SendAsync<T>(HttpRequestMessage req)
        => await SendForNullableAsync<T>(req) ?? throw new Exception("Response body was null");

    public async Task<T?> SendForNullableAsync<T>(HttpRequestMessage req)
    {
        using var res = await SendAsync(req);

        var body = await res.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<T>(body);
    }

    public async Task<string> SendForStringAsync(HttpRequestMessage req)
    {
        using var res = await SendAsync(req);
        return await res.Content.ReadAsStringAsync();
    }

    public async Task<HttpResponseMessage> SendAsync(HttpRequestMessage req, bool throwIfNotSuccessful = true)
    {
        HttpResponseMessage res;
        try
        {
            res = await http.SendAsync(req);
        }
        catch (Exception)
        {
            await RecheckConnectionAsync();
            throw;
        }

        if (throwIfNotSuccessful && (!res.IsSuccessStatusCode))
        {
            try
            {
                var body = await res.Content.ReadAsStringAsync();
                Console.WriteLine($"Responded with {res.StatusCode}: {body}");

                throw new HttpRequestException($"Request failed with status code {res.StatusCode}: {body}");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error reading response body: " + ex.Message);
                throw;
            }
        }

        return res;
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
        http.Dispose();
    }
}
