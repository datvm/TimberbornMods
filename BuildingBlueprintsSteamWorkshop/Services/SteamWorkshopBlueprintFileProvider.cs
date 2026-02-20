namespace BuildingBlueprintsSteamWorkshop.Services;

[MultiBind(typeof(IBlueprintFileProvider))]
public class SteamWorkshopBlueprintFileProvider(
    SteamWorkshopContentProvider wsContentProvider
) : IBlueprintFileProvider
{
    public bool IsLocal => false;

    public IEnumerable<string> GetBlueprintFiles()
    {
        foreach (var folder in wsContentProvider.GetContentDirectories())
        {
            foreach (var file in folder.EnumerateFiles(BuildingBlueprintListingService.FileSearchPattern))
            {
                yield return file.FullName;
            }
        }
    }

}
