
namespace BuildingBlueprintsSteamWorkshop.Models;

public class BlueprintUploadable(
    BlueprintWorkshopSelection selection,
    BlueprintThumbnailPicker thumbnailPicker
) : ISteamWorkshopUploadable
{
    static readonly string[] BlueprintMandatoryTags = ["Mod", "Miscellaneous", IBlueprintWorkshopService.WorkshopTag];

    public ulong? ItemId { get; private set; } = ulong.TryParse(selection.ItemId, out var id) && id > 0 ? id : null;
    public string Name { get; private set; } = "[Building Blueprints] New Blueprint";
    public bool NameIsReadOnly => false;
    public string Description { get; } = "";
    public SteamWorkshopVisibility Visibility { get; private set; }
    public IEnumerable<string> MandatoryTags => BlueprintMandatoryTags;
    public IEnumerable<WorkshopTag> AvailableTags { get; } = [];
    public IEnumerable<string> ChosenTags { get; } = [];
    public string ContentPath => BlueprintUploadContentService.WorkshopTempFolder;
    public Texture2D? Preview { get; set; }
    public string PreviewInfo => "";
    public string? PreviewPath => selection.ThumbnailPath;
    public bool UpdateDescription => true;
    public bool UpdateVisibility => true;
    public bool UpdatePreview => PreviewPath is not null;
    public bool UpdateTags => false;

    public void Clear()
    {
        UnityEngine.Object.Destroy(Preview);
        Preview = null;
        BlueprintUploadContentService.CleanUp();
    }

    public void OnItemCreated(ulong itemId, string name, SteamWorkshopVisibility visibility, IEnumerable<string> tags)
    {
        ItemId = itemId;
        Name = name;
        Visibility = visibility;
    }

    public void OnUpdateFinished(SteamWorkshopUpdateResponse updateResponse) { }

    public void OnUpdateRequestCreated(SteamWorkshopUpdateRequest updateRequest) { }

    public void OnUpdateStarted(string name) { }

    public void RefreshPreview()
    {
        var path = selection.ThumbnailPath = thumbnailPicker.Pick();
        Preview = thumbnailPicker.LoadThumbnail(path);
    }

    public bool ValidateName(string name) => true;
}
