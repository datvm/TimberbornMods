global using System.Runtime.CompilerServices;
global using Timberborn.PrefabGroupSystem;

namespace ModdablePrefab.Management;

[HarmonyPatch]
public static class PrefabModderPatches
{
    static readonly ConditionalWeakTable<GameObject, object?> modifiedPrefabs = [];

    [HarmonyPostfix, HarmonyPatch(typeof(PrefabGroupService), nameof(PrefabGroupService.Load))]
    public static void PatchPrefabs(PrefabGroupService __instance)
    {
        var modder = SpecPrefabModder.Instance;
        if (modder is null) { return; }

        foreach (var prefab in __instance.AllPrefabs)
        {
            // Skip already modified prefabs
            if (modifiedPrefabs.TryGetValue(prefab, out _))
            {
                continue;
            }

            modder.ModifyPrefab(prefab);
            modifiedPrefabs.Add(prefab, null);
        }
    }

}
