namespace Gate.Patches;

[HarmonyPatch]
public static class PrefabPatches
{

    static readonly ImmutableHashSet<string> Prefabs = ["Gate.Folktails", "Gate.IronTeeth"];

    [HarmonyPostfix, HarmonyPatch(typeof(PrefabGroupService), nameof(PrefabGroupService.Load))]
    public static void AddPrefabComponent(PrefabGroupService __instance)
    {
        foreach (var prefab in __instance.AllPrefabs)
        {
            var spec = prefab.GetComponent<PrefabSpec>();

            if (spec && Prefabs.Contains(spec.PrefabName))
            {
                prefab.AddComponent<GateComponentSpec>();
            }
        }
    }

}
