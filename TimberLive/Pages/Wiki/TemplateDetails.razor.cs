
namespace TimberLive.Pages.Wiki;

partial class TemplateDetails
{

    [Parameter, SupplyParameterFromQuery, EditorRequired]
    public string Path { get; set; }

    [Parameter, SupplyParameterFromQuery, EditorRequired]
    public string FactionId { get; set; }

    string? err;
    IParsedLabeledTemplateBlueprint? bp;

    protected override async Task OnParametersSetAsync()
    {
        try
        {
            err = null;

            var httpBp = await BlueprintApi.GetBlueprintAsync(Path);
            var parsed = BlueprintApiService.ParseTemplateBlueprint(httpBp, Path);

            if (parsed is not IParsedLabeledTemplateBlueprint t)
            {
                throw new InvalidOperationException("Blueprint is not a valid building or plant");
            }

            bp = t;
        }
        catch (Exception ex)
        {
            err = "Error loading template: " + ex.Message;
        }
    }

}
