namespace ConfigurableExplosives.Patches;

[HarmonyPatch]
public class PrefabPatches
{

    [HarmonyPostfix, HarmonyPatch(typeof(PrefabGroupService), nameof(PrefabGroupService.Load))]
    public static void PatchDynamitePrefab(PrefabGroupService __instance)
    {
        foreach (var prefab in __instance.AllPrefabs)
        {
            ModifyPrefab(prefab);
        }
    }

    static readonly ImmutableArray<ImmutableHashSet<string>> DynamitePrefabNames = [
        ["Dynamite.Folktails", "Dynamite.IronTeeth"],
        ["DoubleDynamite.Folktails", "DoubleDynamite.IronTeeth"],
        ["TripleDynamite.Folktails", "TripleDynamite.IronTeeth"]
    ];

    const int BaseScienceCost = 600;
    const int StepScienceCost = 300;

    const int StepExtractCost = 1;

    static int CalculateScienceCost(int maxDepth) => BaseScienceCost + (maxDepth - 1) * (MSettings.NoCostIncrease ? 0 : StepScienceCost);

    static int CalculateExtractCost(int maxDepth) => MSettings.NoCostIncrease ? 0 : StepExtractCost * (maxDepth - 1);

    static void ModifyPrefab(GameObject prefab)
    {
        var spec = prefab.GetComponent<PrefabSpec>();
        if (!spec) { return; }

        var depth = GetPrefabDepth(spec);
        if (depth == -1) { return; }

        var dynamite = spec.GetComponentFast<DynamiteSpec>();

        var maxDepth = MSettings.MaxDepths[depth];
        dynamite._depth = maxDepth;

        var building = spec.GetComponentFast<BuildingSpec>();
        building._scienceCost = CalculateScienceCost(maxDepth);
        SetExtractCost(building, maxDepth);
    }

    static int GetPrefabDepth(PrefabSpec spec)
    {

        int depth = -1;
        for (int i = 0; i < DynamitePrefabNames.Length; i++)
        {
            if (!DynamitePrefabNames[i].Contains(spec.PrefabName)) { continue; }

            depth = i;
            break;
        }

        return depth;
    }

    static void SetExtractCost(BuildingSpec building, int maxDepth)
    {
        var cost = building._buildingCost;
        var extractCost = CalculateExtractCost(maxDepth);

        var hasExtract = false;
        for (int i = 0; i < cost.Length; i++)
        {
            var c = cost[i];
            if (c.GoodId != "Extract") { continue; }

            hasExtract = true;
            if (extractCost > 0)
            {
                cost[i] = c with { _amount = CalculateExtractCost(maxDepth), };
            }
            break;
        }

        if (hasExtract && extractCost == 0)
        {
            building._buildingCost = [.. cost.Where(c => c.GoodId != "Extract")];
        }
        else if (!hasExtract && extractCost > 0)
        {
            building._buildingCost =
            [
                .. cost,
                new()
                {
                    _goodId = "Extract",
                    _amount = extractCost,
                },
            ];
        }
    }

}
