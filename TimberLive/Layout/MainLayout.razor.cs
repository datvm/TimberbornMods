namespace TimberLive.Layout;

partial class MainLayout : IDisposable
{

    bool connectedBefore;

    protected override void OnInitialized()
    {
        Api.ConnectionStateChanged += OnConnectionChanged;
    }

    void OnConnectionChanged(object? sender, bool e)
    {
        if (e)
        {
            connectedBefore = true;
        }

        StateHasChanged();
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
        Api.ConnectionStateChanged -= OnConnectionChanged;
    }
}
