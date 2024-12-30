using HarmonyLib;
using Timberborn.ModManagerScene;

namespace ConfigurableGrowth;

public class ModStarter : IModStarter
{

    public void StartMod(IModEnvironment modEnvironment)
    {
        new Harmony(nameof(ConfigurableGrowth)).PatchAll();
    }

}
