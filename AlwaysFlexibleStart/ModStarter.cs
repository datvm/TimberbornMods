using HarmonyLib;
using Timberborn.ModManagerScene;

namespace AlwaysFlexibleStart;
public class ModStarter : IModStarter
{

    public void StartMod(IModEnvironment modEnvironment)
    {
        new Harmony(nameof(AlwaysFlexibleStart)).PatchAll();
    }

}
