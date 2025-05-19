namespace ConfigurableHousing.Patches;

readonly record struct DefaultDwellingValues(string Name,
    int MaxBeavers,
    float Sleep, float Shelter,
    int EntranceZ, ImmutableArray<(Vector3Int, Vector3Int)> AddedEdgeGroups, Vector3 LocalAccess);

[HarmonyPatch]
public static class DwellingPatches
{
    static readonly Dictionary<string, DefaultDwellingValues> DefaultDwellingValues = [];

    [HarmonyPostfix, HarmonyPatch(typeof(PrefabGroupService), nameof(PrefabGroupService.Load))]
    public static void LoadPostfix(PrefabGroupService __instance)
    {
        var shouldRemoveProcreation = MSettings.AddOtherFaction && MSettings.RemoveProcreation;

        var dwelling = __instance.AllPrefabs.SelectMany(q => q.GetComponents<DwellingSpec>());
        foreach (var spec in dwelling)
        {
            var hasBlockSpec = spec.TryGetComponentFast<BlockObjectSpec>(out var blockSpec);
            var hasBlockNavMesh = spec.TryGetComponentFast<BlockObjectNavMeshSettingsSpec>(out var navSpec);
            var hasLocalAccess = spec.TryGetComponentFast<BuildingAccessibleSpec>(out var buildingAccessibleSpec);
            var entrance = blockSpec?.Entrance;
            var hasEntrance = entrance is not null && entrance.Value.HasEntrance;
            int entranceZ = 0;

            if (hasEntrance)
            {
                entranceZ = entrance!.Value.Coordinates.z;
            }
            else
            {
                Debug.LogWarning($"ConfigurableHousing: Dwelling {spec.name} has no block spec or entrance");
            }

            if (!DefaultDwellingValues.TryGetValue(spec.name, out var def))
            {
                var edgeGroups = hasBlockNavMesh ? navSpec._edgeGroups
                    .SelectMany(q => q.AddedEdges
                        .Select(q => (q._start, q._end))
                        .ToImmutableArray())
                    .ToImmutableArray() : [];

                var localAccess = hasLocalAccess ? buildingAccessibleSpec._localAccess : default;

                def = new(spec.name,
                    spec.MaxBeavers,
                    spec._sleepEffects.FirstOrDefault(q => q._needId == "Sleep")?._pointsPerHour ?? 0,
                    spec._sleepEffects.FirstOrDefault(q => q._needId == "Shelter")?._pointsPerHour ?? 0,
                    entranceZ,
                    edgeGroups,
                    localAccess);
                DefaultDwellingValues[spec.name] = def;
            }

            var maxBeavers = Math.Max(1, (int)(def.MaxBeavers * MSettings.MaxBeaverMultiplier));
            spec._maxBeavers = maxBeavers;

            if (hasEntrance)
            {
                var coor = entrance!.Value.Coordinates;
                blockSpec!._entrance._coordinates = coor with { z = MSettings.MoveEntranceFloor ? 0 : def.EntranceZ };
            }

            if (hasBlockNavMesh && navSpec.EdgeGroups.Count > 0)
            {
                var grps = navSpec.EdgeGroups
                    .SelectMany(q => q.AddedEdges)
                    .Zip(def.AddedEdgeGroups, (curr, def) => (curr, def));
                foreach (var (curr, original) in grps)
                {
                    curr._start = original.Item1 with { z = original.Item1.z - (MSettings.MoveEntranceFloor ? def.EntranceZ : 0) };
                    curr._end = original.Item2 with { z = original.Item2.z - (MSettings.MoveEntranceFloor ? def.EntranceZ : 0) };
                }
            }

            if (hasLocalAccess)
            {
                // This one use World-Coordinates, not Z
                buildingAccessibleSpec._localAccess = def.LocalAccess with
                {
                    y = def.LocalAccess.y - (MSettings.MoveEntranceFloor ? def.EntranceZ : 0)
                };
            }

            foreach (var eff in spec._sleepEffects)
            {
                switch (eff._needId)
                {
                    case "Sleep":
                        eff._pointsPerHour = def.Sleep * MSettings.SleepSatisfactionMultiplier;
                        break;
                    case "Shelter":
                        eff._pointsPerHour = def.Shelter * MSettings.ShelterSatisfactionMultiplier;
                        break;
                }
            }

            if (shouldRemoveProcreation)
            {
                var proSpec = spec.GetComponentFast<ProcreationHouseSpec>();
                if (proSpec)
                {
                    UnityEngine.Object.Destroy(proSpec);
                }

                var pro = spec.GetComponentFast<ProcreationHouse>();
                if (pro)
                {
                    UnityEngine.Object.Destroy(pro);
                }
            }
        }
    }


}
