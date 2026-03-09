namespace TimberLive.Pages;

partial class Home
{

    HttpHomePageInfo? model;
    HttpMod[] sortedMods = [];
    bool sortByName;

    string kw = "";

    protected override async Task OnInitializedAsync()
    {
        model = await Api.GetAsync<HttpHomePageInfo>("misc");
        SortMods();
    }

    void SortMods()
    {
        IEnumerable<HttpMod> sorted = model?.Mods ?? [];

        if (sortByName)
        {
            sorted = sorted.OrderBy(m => m.Name);

        }
        
        sortedMods = [.. sorted];
    }

}
