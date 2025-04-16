global using Timberborn.PrefabGroupSystem;

namespace ConfigurableFaction.Patches;

[HarmonyPatch]
public static class FactionPatches
{

    public static Dictionary<string, ImmutableArray<string>> OriginalPaths = [];

    [HarmonyPostfix, HarmonyPatch(typeof(BlueprintDeserializer), nameof(BlueprintDeserializer.DeserializeSpec))]
    public static void DeserializeBlueprint(ref ComponentSpec __result)
    {
        if (FactionBuildingService.Instance is null) { return; }

        switch (__result)
        {
            case FactionSpec factionSpec:
                __result = ModifyFactionSpec(factionSpec);
                break;
            case PrefabGroupSpec prefabGroupSpec:
                __result = ModifyPrefabGroupSpec(prefabGroupSpec);
                break;
        }
    }

    static FactionSpec ModifyFactionSpec(FactionSpec factionSpec)
    {
        List<string> needs = [.. factionSpec.Needs];
        List<string> materialGroups = [.. factionSpec.MaterialGroups];
        List<string> goods = [.. factionSpec.Goods];
        List<string> prefabGroups = [.. factionSpec.PrefabGroups];

        foreach (var otherFacInfo in FactionBuildingService.Instance!.Factions.Values)
        {
            if (!otherFacInfo.Enabled || otherFacInfo.Id == factionSpec.Id) { continue; }
            var (otherFac, _) = otherFacInfo;

            needs.AddRange(otherFac.Needs);
            materialGroups.AddRange(otherFac.MaterialGroups);
            goods.AddRange(otherFac.Goods);

            if (MSettings.AddPlants)
            {
                var naturalResources = otherFac.PrefabGroups.FirstOrDefault(q => q.StartsWith("NaturalResources."));
                if (naturalResources is not null)
                {
                    prefabGroups.Add(naturalResources);
                }
            }
        }

        return factionSpec with
        {
            Needs = [.. needs.Distinct()],
            MaterialGroups = [.. materialGroups.Distinct()],
            Goods = [.. goods.Distinct()],
            PrefabGroups = [..prefabGroups.Distinct()],
        };
    }

    static PrefabGroupSpec ModifyPrefabGroupSpec(PrefabGroupSpec prefabGroupSpec)
    {
        if (prefabGroupSpec.Id.StartsWith("Buildings."))
        {
            return ModifyBuildingGroupSpec(prefabGroupSpec);
        }
        else
        {
            return prefabGroupSpec;
        }
    }

    static PrefabGroupSpec ModifyBuildingGroupSpec(PrefabGroupSpec prefabGroupSpec)
    {
        OriginalPaths[prefabGroupSpec.Id] = prefabGroupSpec.Paths;
        List<string> allBuildings = [.. prefabGroupSpec.Paths];

        var removeDup = MSettings.TryRemovingDuplicates;
        foreach (var facInfo in FactionBuildingService.Instance!.Factions.Values)
        {
            if (!facInfo.Enabled) { continue; }
            var (fac, buildings) = facInfo;

            if (buildings.Length == 0 || fac.PrefabGroups.Contains(prefabGroupSpec.Id)) { continue; }

            allBuildings.AddRange(buildings
                .Where(q => q.Enabled && (!removeDup || !q.IsCommon))
                .Select(q => q.Id));
        }

        return prefabGroupSpec with { Paths = [.. allBuildings.Distinct()] };
    }

}
