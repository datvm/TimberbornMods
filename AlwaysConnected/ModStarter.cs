using HarmonyLib;
using Timberborn.ModManagerScene;

namespace AlwaysConnected;
public class ModStarter : IModStarter
{

    public void StartMod(IModEnvironment modEnvironment)
    {
        new Harmony(nameof(AlwaysConnected)).PatchAll();
    }

}
