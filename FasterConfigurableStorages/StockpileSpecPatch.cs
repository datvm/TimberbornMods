using Timberborn.Stockpiles;

namespace FasterConfigurableStorages;

[HarmonyPatch(typeof(StockpileSpec), nameof(StockpileSpec.MaxCapacity), MethodType.Getter)]
public static class StockpileSpecPatch
{

    public static void Postfix(ref int __result)
    {
        __result = (int)Math.Ceiling(__result * ModSettings.StorageCapacityMultiplier);
    }

}
