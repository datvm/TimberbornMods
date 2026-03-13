namespace TimberLive.Components;

partial class DataFetcherPresentation
{

    [Parameter, EditorRequired]
    public RefreshableDataFetcher? Fetcher { get; set; }
    RefreshableDataFetcher? currFetcher;

    bool autoLoading;

    protected override void OnParametersSet()
    {
        autoLoading = Fetcher?.IsRunning == true;

        if (currFetcher != Fetcher)
        {
            currFetcher?.StateChanged -= OnFetcherStateChanged;

            currFetcher = Fetcher;
            currFetcher?.StateChanged += OnFetcherStateChanged;
        }
    }

    void OnFetcherStateChanged() => StateHasChanged();

    void OnAutoLoadingChanged()
    {
        if (currFetcher is null) { return; }

        if (autoLoading)
        {
            currFetcher.Start();
        }
        else
        {
            currFetcher.Pause();
        }
    }

    void Refresh()
    {
        if (Fetcher is null) { return; }
        Fetcher.RefreshOnce();
    }

}
