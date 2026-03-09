
namespace TimberLive.Layout;

partial class MainLayout : IDisposable
{

    bool connectedBefore;

    [CascadingParameter]
    public RouteData RouteData { get; set; } = null!;

    public bool NeedConnection 
        => !Api.Connected && RouteData.PageType.GetCustomAttribute<NoConnectionRequiredAttribute>() is null;

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
