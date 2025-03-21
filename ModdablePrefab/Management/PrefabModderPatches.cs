global using Timberborn.PrefabGroupSystem;

namespace ModdablePrefab.Management;

[HarmonyPatch]
public static class PrefabModderPatches
{

    [HarmonyPostfix, HarmonyPatch(typeof(PrefabGroupService), nameof(PrefabGroupService.Load))]
    public static void PatchPrefabs(PrefabGroupService __instance)
    {
        var modder = SpecPrefabModder.Instance;
        if (modder is null) { return; }

        foreach (var prefab in __instance.AllPrefabs)
        {
            modder.ModifyPrefab(prefab);
        }
    }

}
