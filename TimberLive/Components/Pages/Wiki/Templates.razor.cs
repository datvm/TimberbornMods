namespace TimberLive.Components.Pages.Wiki;

partial class Templates
{

    FactionTemplateCompilation? templates;

    string selectingFactionId = "";

    protected override async Task OnInitializedAsync()
    {
        templates = await SpecApi.GetFactionTemplatesAsync();

        selectingFactionId = templates.Factions.First().Id;
    }

    public async Task ExportBuildingAsync()
    {
        var data = await ExportService.ExportWikiBuildings(templates!);
        await DownloadService.DownloadFileAsync(data, "wiki-buildings.zip");
    }

}
