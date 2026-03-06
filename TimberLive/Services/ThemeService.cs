namespace TimberLive.Services;

[SelfService(Lifetime = ServiceLifetime.Singleton)]
public class ThemeService(IJSRuntime js)
{
    public bool DarkTheme { get; private set; }
    public event EventHandler<bool>? ThemeChanged;

    public async Task InitAsync()
    {
        DarkTheme = await js.InvokeAsync<bool>("BlazorHelper.isDarkModePreferred");
    }

    public async Task SetThemeAsync(bool? dark = null)
    {
        var value = dark ?? !DarkTheme;
        DarkTheme = value;

        await js.InvokeVoidAsync("BlazorHelper.setTheme", value);
        ThemeChanged?.Invoke(this, value);
    }

}
