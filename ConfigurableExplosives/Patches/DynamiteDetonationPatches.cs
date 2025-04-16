global using ConfigurableExplosives.UI;

namespace ConfigurableExplosives.Patches;

[HarmonyPatch]
public static class DynamiteDetonationPatches
{

    [HarmonyPrefix, HarmonyPatch(typeof(Dynamite), nameof(Dynamite.Tick))]
    public static bool PreTick() => false;

    [HarmonyPrefix, HarmonyPatch(typeof(Dynamite), nameof(Dynamite.Depth), MethodType.Getter)]
    public static bool PatchDepth(Dynamite __instance, ref int __result)
    {
        var comp = __instance.GetComponentFast<ConfigurableDynamiteComponent>();
        if (!comp) { return true; }

        __result = comp.DetonationDepth;
        return false;
    }

    [HarmonyPrefix, HarmonyPatch(typeof(Dynamite), nameof(Dynamite.TriggerNeighbors))]
    public static bool PatchTriggerNeighbors(Dynamite __instance)
    {
        var comp = __instance.GetComponentFast<ConfigurableDynamiteComponent>();
        if (!comp) { return true; }

        if (comp.TriggerRadius < 1) { return false; }

        var rad = comp.TriggerRadius;
        var center = __instance._blockObject.Coordinates;
        var startX = center.x - rad;
        var startY = center.y - rad;
        var endX = center.x + rad;
        var endY = center.y + rad;
        var startZ = center.z - rad;
        var endZ = center.z + rad;

        for (int x = startX; x <= endX; x++)
        {
            for (int y = startY; y <= endY; y++)
            {
                for (int z = startZ; z <= endZ; z++)
                {
                    var coord = new Vector3Int(x, y, z);
                    if (coord == center) { continue; }

                    var d = __instance._blockService.GetBottomObjectComponentAt<Dynamite>(coord);
                    if (d && d.enabled)
                    {
                        d.Trigger();
                    }
                }
            }
        }

        return false;
    }

}
