namespace TimberLive.Services;

public class Loc(ApiService api) : IApiConnectionListener
{
    Dictionary<string, string> keys = [];

    public async Task OnConnectedAsync()
    {
        keys = await api.GetAsync<Dictionary<string, string>>("loc");
    }

    public async Task OnDisconnectedAsync()
    {
        keys = [];
    }

    public string T(string key) => keys.TryGetValue(key, out var v) ? v : key;

}
