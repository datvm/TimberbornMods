global using System.Runtime.CompilerServices;
global using Timberborn.Growing;
global using Timberborn.Yielding;

namespace ConfigurableGrowth.Patches;

[HarmonyPatch(typeof(Growable), nameof(Growable.GrowthTimeInDays), MethodType.Getter)]
public static class GrowablePatch
{
    static readonly ConditionalWeakTable<GrowableSpec, object?> ModifiedSpecs = [];
    static readonly FieldInfo growableGrowthDays = typeof(GrowableSpec).GetField("_growthTimeInDays", BindingFlags.NonPublic | BindingFlags.Instance);

    public static void Prefix(Growable __instance, GrowableSpec ____growableSpec)
    {
        if (ModifiedSpecs.TryGetValue(____growableSpec, out _)) { return; }
        ModifiedSpecs.Add(____growableSpec, null);

        var yielders = __instance.GetComponentsInParent<Yielder>();
        var logYielder = yielders.FirstOrDefault(IsYieldLog);
        var isTree = logYielder is not null;

        var mul = isTree ? ModSettings.TreeGrowthRate : ModSettings.CropGrowthRate;
        growableGrowthDays.SetValue(____growableSpec, (float)growableGrowthDays.GetValue(____growableSpec) / mul);
    }

    static bool IsYieldLog(Yielder yielder) => yielder.YielderSpec.Yield.GoodId == "Log";

}
