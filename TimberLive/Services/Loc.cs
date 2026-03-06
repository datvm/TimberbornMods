namespace TimberLive.Services;

[SelfService(Lifetime = ServiceLifetime.Singleton)]
public class Loc
{
    readonly ApiService api;
    Dictionary<string, string> keys = [];

    public Loc(ApiService api)
    {
        this.api = api;

        api.RegisterConnectedCallback(LoadAsync);
    }

    public async Task LoadAsync()
    {
        keys = await api.GetAsync<Dictionary<string, string>>("loc");
    }

    public string T(string key) => keys.TryGetValue(key, out var v) ? v : key;

}
