using HarmonyLib;
using Timberborn.ModManagerScene;

namespace HealthyBeavers;
public class ModStarter : IModStarter
{

    public void StartMod(IModEnvironment modEnvironment)
    {
        new Harmony(nameof(HealthyBeavers)).PatchAll();
    }

}
