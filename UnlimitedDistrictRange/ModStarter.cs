using HarmonyLib;
using Timberborn.ModManagerScene;

namespace UnlimitedDistrictRange;
internal class ModStarter : IModStarter
{

    public void StartMod(IModEnvironment modEnvironment)
    {
        new Harmony(nameof(UnlimitedDistrictRange)).PatchAll();
    }

}
