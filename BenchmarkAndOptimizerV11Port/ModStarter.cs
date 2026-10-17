using System.Collections;
using System.Reflection;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using Newtonsoft.Json;
using HarmonyLib;
using Timberborn.ModManagerScene;

namespace BenchmarkAndOptimizerV11;

public sealed class ModStarter : IModStarter
{
    public void StartMod(IModEnvironment modEnvironment)
    {
        var harmony = new Harmony("tharbad08.BenchmarkAndOptimizerV11Port");
        Runtime.Initialize(modEnvironment.ModPath, harmony);

        Runtime.Log($"loaded; config={Runtime.ConfigPath}; normal throttling uses direct method patches");
    }
}
