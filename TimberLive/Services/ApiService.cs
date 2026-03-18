namespace TimberLive.Services;

[SelfService(Lifetime = ServiceLifetime.Singleton)]
public class ApiService : IDisposable
{
    readonly JsonSerializerOptions jsonOptions = new()
    {
        IncludeFields = true, // For tuples
    };

    HttpClient http = new();
    static readonly TimeSpan Timeout = TimeSpan.FromSeconds(5);

    bool rechecking;

    public bool Connected { get; private set; }
    public event EventHandler<bool>? ConnectionStateChanged;

    readonly HashSet<IApiConnectionListener> connectionCallbacks = [];

    public Uri CurrentUri { get; private set; } = new("http://localhost:8080");

    public void RegisterConnectionListener(IApiConnectionListener listener)
    {
        connectionCallbacks.Add(listener);
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
            BaseAddress = new(uri, MoreHttpApiUtils.EndpointStart + "/"),
            Timeout = Timeout,            
        };
        CurrentUri = uri;

        if (!await PingAsync())
        {
            return "Ping failed. Please make sure the API is on.";
        }

        Connected = true;

        foreach (var c in connectionCallbacks)
        {
            await c.OnConnectedAsync();
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
                await DisconnectAsync();
            }
        }
        finally
        {
            rechecking = false;
        }
    }

    public async Task DisconnectAsync()
    {
        Connected = false;

        foreach (var c in connectionCallbacks)
        {
            await c.OnDisconnectedAsync();
        }

        ConnectionStateChanged?.Invoke(this, false);
    }

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

    public async Task<string> GetStringAsync(string path)
        => await SendForStringAsync(new HttpRequestMessage(HttpMethod.Get, path));

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
        return JsonSerializer.Deserialize<T>(body, jsonOptions);
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

                throw new HttpRequestException($"Request failed with status code {res.StatusCode}: {body}", null, res.StatusCode);
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
