namespace TimberLive.Components.Pages;

[NoConnectionRequired]
partial class Settings
{

    bool initialized;
    string? error;

    static readonly Dictionary<StorageKey, ValueTuple<string, string>> SettingUI = new()
    {
        { StorageKey.RefreshTime, ("Refresh time", "") },
    };

    readonly Dictionary<StorageKey, string> values = [];

    protected override void OnInitialized()
    {
        Storage.GetAllRawValues(values);
        initialized = true;
    }

    void Submit()
    {
        error = Validate();
        if (error is not null) { return; }

        SetValue(StorageKey.RefreshTime, int.Parse);

        void SetValue<T>(StorageKey key, Func<string, T> value) where T : notnull
        {
            Storage.SetValue(key, value(values[key]));
        }
    }

    string? Validate()
    {
        if (!int.TryParse(values[StorageKey.RefreshTime], out var rt)
            || rt < 1)
        {
            return "Refresh time must be a positive integer.";
        }

        return null;
    }

}
