using HarmonyLib;
using Timberborn.ModManagerScene;

namespace InstantBuild;

public class ModStarter : IModStarter
{

    public void StartMod(IModEnvironment modEnvironment)
    {
        new Harmony(nameof(InstantBuild)).PatchAll();
    }

}
