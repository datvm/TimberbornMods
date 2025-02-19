global using Timberborn.NaturalResourcesReproduction;
using Timberborn.BlockSystem;

namespace ConfigurableGrowth.Patches;

[HarmonyPatch]
public class ReproduciblePatches
{

    [HarmonyPostfix, HarmonyPatch(typeof(ReproducibleSpec), nameof(ReproducibleSpec.ReproductionChance), MethodType.Getter)]
    public static void PatchReproductionChance(ref float __result)
    {
        if (ModSettings.ReproducibleChanceMultiplier != 1)
        {
            __result *= ModSettings.ReproducibleChanceMultiplier;
        }
    }

    [HarmonyPrefix, HarmonyPatch(typeof(NaturalResourceReproducer), nameof(NaturalResourceReproducer.MarkSpots))]
    public static bool PatchMarkSpots(Reproducible reproducible, NaturalResourceReproducer __instance)
    {
        var ver = ModSettings.SpreadVertically;
        var d = ModSettings.SpreadDistance;

        if (!ver && d == 1) { return true; }

        var comp = reproducible.GetComponentFast<BlockObject>();
        for (int x = -d; x <= d; x++)
        {
            for (int y = -d; y <= d; y++)
            {
                if (x == 0 && y == 0) { continue; }

                if (ver)
                {
                    for (int z = -d; z <= d; z++)
                    {
                        MarkAtCoord(new(comp.Coordinates.x + x, comp.Coordinates.y + y, comp.Coordinates.z + z));
                    }
                }
                else
                {
                    MarkAtCoord(new(comp.Coordinates.x + x, comp.Coordinates.y + y, comp.Coordinates.z));
                }
            }
        }

        return false;

        void MarkAtCoord(Vector3Int coord)
        {
            __instance.InitializeArray(reproducible);
            __instance._potentialSpots[NaturalResourceReproducer.ReproducibleKey.Create(reproducible)].Add(coord);
        }
    }

    [HarmonyPrefix, HarmonyPatch(typeof(NaturalResourceReproducer), nameof(NaturalResourceReproducer.CanReproduceAtCoordinates))]
    public static bool PatchCanReproduceAtCoordinates(string id, Vector3Int coordinates, ref bool __result, NaturalResourceReproducer __instance)
    {
        var ver = ModSettings.SpreadVertically;
        var d = ModSettings.SpreadDistance;

        if (!ver && d == 1) { return true; }

        for (int x = -d; x <= d; x++)
        {
            for (int y = -d; y <= d; y++)
            {
                if (x == 0 && y == 0) { continue; }

                bool found = false;

                if (ver)
                {
                    for (int z = -d; z <= d; z++)
                    {
                        if (CheckAtCoord(new(coordinates.x + x, coordinates.y + y, coordinates.z + z)))
                        {
                            found = true;
                            break;
                        }
                    }
                }
                else
                {
                    if (CheckAtCoord(new(coordinates.x + x, coordinates.y + y, coordinates.z)))
                    {
                        found = true;
                    }
                }

                if (found)
                {
                    __result = true;
                    return false;
                }
            }
        }

        __result = false;
        return false;

        bool CheckAtCoord(Vector3Int coord)
        {
            var repro = __instance._blockService.GetBottomObjectComponentAt<Reproducible>(coord);
            return repro is not null
                && repro.Id == id
                && !repro.ReproductionDisabled
                && repro.GetComponentFast<BlockObject>().Coordinates.z == coord.z;

        }
    }

}
