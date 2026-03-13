namespace TimberLive.Services;

[SelfService(Lifetime = ServiceLifetime.Singleton)]
public class ThemeService(IJSRuntime js, StorageService storage)
{
    public bool DarkTheme { get; private set; }
    public event EventHandler<bool>? ThemeChanged;

    public async Task InitAsync()
    {
        bool? storedValue = storage.HasValue(StorageKey.DarkMode) ? storage.GetValue<bool>(StorageKey.DarkMode) : null;
        storedValue ??= await js.InvokeAsync<bool>("BlazorHelper.isDarkModePreferred");

        DarkTheme = storedValue.Value;
        await SetThemeAsync(DarkTheme);
    }

    public async Task SetThemeAsync(bool? dark = null)
    {
        var value = dark ?? !DarkTheme;
        DarkTheme = value;
        storage.SetValue(StorageKey.DarkMode, value);

        await js.InvokeVoidAsync("BlazorHelper.setTheme", value);
        ThemeChanged?.Invoke(this, value);
    }

}
