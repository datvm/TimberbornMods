namespace BuildingBlueprintsSteamWorkshop.Services;

[MultiBind(typeof(IBlueprintWorkshopService))]
public class BlueprintSteamWorkshopService(
    SteamManager steamManager,
    SteamWorkshopUploadPanel uploadPanel,
    BlueprintThumbnailPicker thumbnailPicker
) : IBlueprintWorkshopService
{

    public bool IsSupported => steamManager.Initialized;
    public string? WorkshopBrowsingUrl => "https://steamcommunity.com/workshop/browse/?appid=1062090&requiredtags[]=BuildingBlueprints";

    public void OpenUploadBlueprint(BlueprintWorkshopSelection selection)
    {
        BlueprintUploadContentService.PrepareContent(selection.BlueprintPaths);

        var uploadable = new BlueprintUploadable(selection, thumbnailPicker);
        uploadPanel.Open(uploadable);
    }

}
