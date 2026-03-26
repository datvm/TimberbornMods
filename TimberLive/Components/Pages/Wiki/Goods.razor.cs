namespace TimberLive.Components.Pages.Wiki;

partial class Goods
{

    List<ParsedGoodSpec>? specs;

    protected override async Task OnInitializedAsync()
    {
        specs = await SpecApi.GetSpecsAsync<ParsedGoodSpec>();
    }

}
