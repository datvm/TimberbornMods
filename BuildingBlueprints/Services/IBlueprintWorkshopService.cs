namespace BuildingBlueprints.Services;

public interface IBlueprintWorkshopService
{
    const string WorkshopTag = "BuildingBlueprints";

    bool IsSupported { get; }
    void OpenUploadBlueprint(BlueprintWorkshopSelection selection);
    string? WorkshopBrowsingUrl { get; }

}
