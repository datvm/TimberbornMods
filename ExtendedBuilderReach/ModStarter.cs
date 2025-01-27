using HarmonyLib;
using Timberborn.ModManagerScene;

namespace ExtendedBuilderReach;


public class ModStarter : IModStarter
{

    public void StartMod(IModEnvironment modEnvironment)
    {
        new Harmony(nameof(ExtendedBuilderReach)).PatchAll();
    }

}
