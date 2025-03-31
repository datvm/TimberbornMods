namespace TImprove4Modders.Services;

#pragma warning disable CS9113 // For DI to ensure this one load after MSettings
public class ModManagementService(MSettings _, ModListView lstMods) : ILoadableSingleton
#pragma warning restore CS9113 // Parameter is unread.
{
    public void Load()
    {
        if (MSettings.BetterModOrder)
        {
            Debug.Log("Resorting mods");
            lstMods.SortList();
        }
    }
}
