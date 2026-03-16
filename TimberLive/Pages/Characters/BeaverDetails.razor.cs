namespace TimberLive.Pages.Characters;

partial class BeaverDetails
{

    [Parameter]
    public Guid Id { get; set; }

    RefreshableDataFetcher? refresher;
    HttpCharacterDetailed? character;

    protected override async Task OnInitializedAsync()
    {
        refresher = RefreshableDataFetcher.Create(LoadAsync, Storage);
    }

    async Task LoadAsync()
    {
        character = await Api.GetAsync<HttpCharacterDetailed>($"characters/{Uri.EscapeDataString(Id.ToString())}");
    }

}
