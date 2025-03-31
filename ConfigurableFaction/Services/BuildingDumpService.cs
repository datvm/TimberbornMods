global using Timberborn.BlockSystem;
global using Timberborn.EntitySystem;
global using Timberborn.GameFactionSystem;
global using Timberborn.PrefabSystem;
using ConfigurableFaction.Patches;

namespace ConfigurableFaction.Services;

public class BuildingDumpService(
    PrefabGroupService prefabs,
    FactionBuildingService infoService,
    FactionService factions,
    ISpecService specs,
    ILoc t
) : ILoadableSingleton
{

    public void Load()
    {
        var currFaction = factions.Current;

        var prefabGroupName = currFaction.PrefabGroups.FirstOrDefault(q => q.StartsWith("Buildings."));
        if (prefabGroupName is null)
        {
            Debug.LogWarning($"Faction {factions.Current.Id} does not have any {nameof(factions.Current.PrefabGroups)} that starts with \"Buildings.\".");
            return;
        }

        var paths = GetPrefabPaths(prefabGroupName);
        if (paths is null) { return; }

        var buildings = prefabs.AllPrefabs.SelectMany(q => q.GetComponents<PlaceableBlockObjectSpec>());
        List<SimpleBuildingSpec> simpleSpecs = [];
        foreach (var b in buildings)
        {
            if (b.DevModeTool) { continue; }

            var prefabSpec = b.GetComponentFast<PrefabSpec>();
            if (!prefabSpec) { continue; }

            var labelSpec = b.GetComponentFast<LabeledEntitySpec>();
            if (!labelSpec) { continue; }

            var validPaths = paths.Where(q => q.EndsWith(prefabSpec.Name)).ToArray();
            if (validPaths.Length != 1)
            {
                if (validPaths.Length > 1) // == 0 means it's added by the mod so don't log warnings
                {
                    Debug.LogWarning($"Cannot find an appropriate path for prefab {prefabSpec.Name}. It has {validPaths.Length} matching items.");
                }

                continue;
            }

            simpleSpecs.Add(new(validPaths.First(), labelSpec.DisplayNameLocKey, false));
        }

        var sorter = new SpecNameComparer(t);
        simpleSpecs.Sort(sorter);

        infoService.PopulateFactionBuildingInfo(factions.Current.Id, simpleSpecs);
    }

    List<string>? GetPrefabPaths(string prefabGroupName)
    {
        if (!FactionPatches.OriginalPaths.TryGetValue(prefabGroupName, out var grp))
        {
            Debug.LogWarning($"Could not find prefab group with id {prefabGroupName}.");
            return null;
        }

        return [.. grp];
    }

    class SpecNameComparer(ILoc t) : IComparer<SimpleBuildingSpec>
    {

        readonly Dictionary<string, string> cache = [];

        public int Compare(SimpleBuildingSpec x, SimpleBuildingSpec y)
        {
            var name1 = cache.GetOrAdd(x.NameKey, () => t.T(x.NameKey));
            var name2 = cache.GetOrAdd(y.NameKey, () => t.T(y.NameKey));

            return string.Compare(name1, name2, StringComparison.Ordinal);
        }
    }

}
