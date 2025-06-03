
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
    }

}
