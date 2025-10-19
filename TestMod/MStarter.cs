namespace TestMod;

public class MStarter : IModStarter
{

    void IModStarter.StartMod(IModEnvironment modEnvironment)
    {
        var h = new Harmony(nameof(TestMod));
        h.PatchAll();
    }


}

[HarmonyPatch]
public static class TestPatch
{

    [HarmonyPostfix, HarmonyPatch(typeof(PrefabGroupService), nameof(PrefabGroupService.Load))]
    public static void LoadPostfix(PrefabGroupService __instance)
    {
        var myP = __instance._assetLoader.Load<GameObject>("Buildings/Wood/PaperMill/PaperMill.IronTeeth");
        var comps = myP.GetComponents(typeof(Component));

        Debug.Log("PaperMill.IronTeeth components:");
        foreach (var c in comps)
        {
            Debug.Log("- " + c);

            if (c is PrefabSpec pre)
            {
                Debug.Log("* Prefab Name: " + pre.PrefabName);
            }
        }

        List<(PrefabSpec, PlaceableBlockObjectSpec)> placables = [];

        foreach (var p in __instance.AllPrefabs)
        {
            var prefabSpec = p.GetComponent<PrefabSpec>();
            if (!prefabSpec) { continue; }

            var placable = p.GetComponent<PlaceableBlockObjectSpec>();
            if (prefabSpec.PrefabName == "PaperMill.IronTeeth")
            {
                Debug.Log("Found it!!!");
                Debug.Log(placable);
            }

            if (!placable) { continue; }

            placables.Add((prefabSpec, placable));
        }

        placables = [.. placables
            .OrderBy(q => q.Item2.ToolGroupId)
            .ThenBy(q => q.Item2.ToolOrder)];

        foreach (var (prefab, placable) in placables)
        {
            Debug.Log($"Prefab: {prefab.name}, Tool: {placable.ToolGroupId} ({placable.ToolOrder})");
        }
    }

}
