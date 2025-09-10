namespace ModdableTimberborn.MechanicalSystem.Patches;

[HarmonyPatchCategory(ModdableMechanicalSystemConfigurator.PatchCategoryName), HarmonyPatch(typeof(MechanicalGraph))]
public class MechanicalGraphPatches
{

    [HarmonyPostfix, HarmonyPatch(nameof(MechanicalGraph.PowerIsSupplied), MethodType.Getter)]
    public static void PowerIsSuppliedWhenZeroConsumption(MechanicalGraph __instance, ref bool __result)
    {
        if (__result) { return; }

        if (__instance.CurrentPower.PowerDemand == 0)
        {
            __result = true;
        }
    }

}
