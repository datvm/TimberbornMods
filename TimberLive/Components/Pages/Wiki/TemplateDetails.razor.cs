namespace TimberLive.Components.Pages.Wiki;

partial class TemplateDetails
{

    [Parameter, SupplyParameterFromQuery, EditorRequired]
    public string Path { get; set; }

    [Parameter, SupplyParameterFromQuery, EditorRequired]
    public string FactionId { get; set; }

    string? err;
    IParsedLabeledTemplateBlueprint? bp;
    Breadcrumb breadcrumb = Breadcrumb.WikiTemplateDetails;

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
            breadcrumb = Breadcrumb.WikiTemplates.CreateChild(T.T(bp.DisplayNameLoc));
        }
        catch (Exception ex)
        {
            err = "Error loading template: " + ex.Message;
        }
    }

}
