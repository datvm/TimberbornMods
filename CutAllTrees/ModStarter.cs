using HarmonyLib;
using Timberborn.ModManagerScene;

namespace CutAllTrees;

public class ModStarter : IModStarter
{

    public void StartMod(IModEnvironment modEnvironment)
    {
        new Harmony(nameof(CutAllTrees)).PatchAll();
    }

}
