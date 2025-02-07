using Timberborn.BlockSystem;

namespace WaterSourcesDontCrash;

[HarmonyPatch]
public static class BlockObjectPatch
{

    [HarmonyPostfix, HarmonyPatch(typeof(BlockSpecification), "MatterBelow", MethodType.Getter)]
    public static void GroundOnlyPrefix(ref MatterBelow __result)
    {
        if (__result == MatterBelow.Ground)
        {
            __result = MatterBelow.GroundOrStackable;
        }
    }

}