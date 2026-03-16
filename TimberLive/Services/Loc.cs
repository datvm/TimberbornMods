namespace TimberLive.Services;

public class Loc : IApiConnectionListener
{
    readonly ApiService api;
    Dictionary<string, string> keys = [];

    public Loc(ApiService api)
    {
        this.api = api;

        api.RegisterConnectedCallback(this);
    }

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
