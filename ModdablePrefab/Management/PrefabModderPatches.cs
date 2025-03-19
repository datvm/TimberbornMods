global using Timberborn.PrefabGroupSystem;

namespace ModdablePrefab.Management;

[HarmonyPatch]
public static class PrefabModderPatches
{

    [HarmonyPostfix, HarmonyPatch(typeof(PrefabGroupService), nameof(PrefabGroupService.Load))]
    public static void PatchPrefabs(PrefabGroupService __instance)
    {
        var registry = PrefabModderRegistry.ModdersByTypes;
        if (registry is null || registry.Count == 0) { return; }

        foreach (var prefab in __instance.AllPrefabs)
        {
            var comps = prefab.GetComponents<BaseComponent>();
            foreach (var comp in comps)
            {
                if (!registry.TryGetValue(comp.GetType(), out var modders)) { continue; }

                foreach (var modder in modders)
                {
                    modder.ModifyPrefab(comp);
                }
            }
        }
    }

}
