
namespace Ziporter.Patches;

[HarmonyPatch]
public static class PrefabPatches
{
    const string ZiporterPrefabId = "Ziporter.Folktails";


    [HarmonyPostfix, HarmonyPatch(typeof(PrefabGroupService), nameof(PrefabGroupService.Load))]
    public static void UpdatePrefabsForZiporter(PrefabGroupService __instance)
    {
        foreach (var prefab in __instance.AllPrefabs)
        {
            var spec = prefab.GetComponent<PrefabSpec>();
            if (!spec || spec.PrefabName != ZiporterPrefabId) { continue; }

            UpdateZiporterPrefab(prefab);
        }
    }

    static void UpdateZiporterPrefab(GameObject prefab)
    {
        prefab.AddComponent<ZiporterSpec>();

        // Correct the Transput
        prefab.AddComponent<MechanicalConnectorReceiverSpec>();
        var transputs = prefab.AddComponent<TransputProviderSpec>();

        transputs._transputSpecs = [
            new(new(0,0,0), Directions3D.Left | Directions3D.Down),
            new(new(2,0,0), Directions3D.Right | Directions3D.Down),
            new(new(0,1,0), Directions3D.Left | Directions3D.Up),
            new(new(2,1,0), Directions3D.Right | Directions3D.Up),
            new(new(1,1,0), Directions3D.Up),
            new(new(1,1,1), Directions3D.Up),
        ];
    }

}
