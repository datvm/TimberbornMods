namespace TimberLive.Components;

partial class Image
{

    [Parameter, EditorRequired]
    public string? Path { get; set; }

    [Parameter(CaptureUnmatchedValues = true)]
    public Dictionary<string, object>? Attributes { get; set; }

    string? imgData;

    protected override async Task OnParametersSetAsync()
    {
        imgData = null;
        StateHasChanged();

        if (!string.IsNullOrEmpty(Path))
        {
            imgData = await Images.GetImageAsync(Path);
        }        
    }

}
