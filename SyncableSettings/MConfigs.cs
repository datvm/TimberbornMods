global using SyncableSettings.Models;
global using SyncableSettings.Services;

namespace SyncableSettings;

public class MStarter : IModStarter
{

    void IModStarter.StartMod(IModEnvironment modEnvironment)
    {
        new Harmony(nameof(SyncableSettings)).PatchAll();
    }

}
