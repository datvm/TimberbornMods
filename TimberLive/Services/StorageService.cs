namespace TimberLive.Services;

[SelfService(Lifetime = ServiceLifetime.Singleton)]
public class StorageService(IJSInProcessRuntime js)
{
    static readonly Dictionary<StorageKey, object> DefaultValues = new()
    {
        { StorageKey.RefreshTime, 5 },
    };

    public static readonly ImmutableArray<StorageKey> AllKeys = [..Enum.GetValues<StorageKey>()
        .Except([ StorageKey.DarkMode ])
        .OrderBy(q => q.ToString())];

    public void Initialize()
    {
        var keys = GetAllValues();

        foreach (var (k, v) in DefaultValues)
        {
            if (!keys.Contains(k.ToString()))
            {
                SetValue(k, v);
            }
        }
    }

    public bool HasValue(StorageKey key)
        => js.Invoke<string?>("localStorage.getItem", key.ToString()) is not null;

    public T? GetValue<T>(StorageKey key) => GetValue<T>(key.ToString());
    public void SetValue<T>(StorageKey key, T value) where T : notnull
        => SetValue(key.ToString(), value);

    public void GetAllRawValues(Dictionary<StorageKey, string> result)
    {
        result.Clear();

        foreach (var k in AllKeys)
        {
            var value = GetRawValue(k.ToString()) ?? "";
            result[k] = value;
        }
    }

    HashSet<string> GetAllValues()
    {
        using var ls = js.GetValue<IJSInProcessObjectReference>("localStorage");
        return [.. js.Invoke<string[]>("Object.keys", ls)];
    }

    string? GetRawValue(string key) => js.Invoke<string?>("localStorage.getItem", key);

    T GetValue<T>(string key)
    {
        var raw = GetRawValue(key) ?? throw new ArgumentNullException($"Settings {key} is not set. This should not be happening.");

        return typeof(T) == typeof(string) ? (T)(object)raw : JsonSerializer.Deserialize<T>(raw)!;
    }

    void SetValue<T>(string key, T value) where T : notnull
    {
        var raw = typeof(T) == typeof(string) ? value?.ToString() : JsonSerializer.Serialize((object)value);
        js.InvokeVoid("localStorage.setItem", key, raw ?? "");
    }

}

public enum StorageKey
{
    DarkMode,
    RefreshTime,
}