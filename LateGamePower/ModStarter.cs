using HarmonyLib;
using Timberborn.ModManagerScene;

namespace LateGamePower;
public class ModStarter : IModStarter
{

    public void StartMod(IModEnvironment modEnvironment)
    {
        new Harmony(nameof(LateGamePower)).PatchAll();
    }

}
