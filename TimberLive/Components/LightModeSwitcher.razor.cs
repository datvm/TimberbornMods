namespace TimberLive.Components;

partial class LightModeSwitcher : IDisposable
{
    public void Dispose()
    {
        GC.SuppressFinalize(this);
        Theme.ThemeChanged -= OnThemeChanged;
    }

    protected override async Task OnInitializedAsync()
    {
        await Theme.InitAsync();
        Theme.ThemeChanged += OnThemeChanged;
    }

    void OnThemeChanged(object? sender, bool e)
    {
        StateHasChanged();
    }

}
