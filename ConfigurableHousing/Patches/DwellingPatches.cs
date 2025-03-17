using Timberborn.BlockSystem;
using Timberborn.DwellingSystem;
using Timberborn.PrefabGroupSystem;
namespace ConfigurableHousing.Patches;

readonly record struct DefaultDwellingValues(string Name, int MaxBeavers, float Sleep, float Shelter, int EntranceZ);

[HarmonyPatch]
public static class DwellingPatches
{
    static readonly Dictionary<string, DefaultDwellingValues> DefaultDwellingValues = [];

    [HarmonyPostfix, HarmonyPatch(typeof(PrefabGroupService), nameof(PrefabGroupService.Load))]
    public static void LoadPostfix(PrefabGroupService __instance)
    {
        var dwelling = __instance.AllPrefabs.SelectMany(q => q.GetComponents<DwellingSpec>());
        Debug.Log($"ConfigurableHousing: Found {dwelling.Count()} dwelling specs");
        foreach (var spec in dwelling)
        {
            var hasBlockSpec = spec.TryGetComponentFast<BlockObjectSpec>(out var blockSpec);
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
                def = new(spec.name,
                    spec.MaxBeavers,
                    spec._sleepEffects.FirstOrDefault(q => q._needId == "Sleep")?._pointsPerHour ?? 0,
                    spec._sleepEffects.FirstOrDefault(q => q._needId == "Shelter")?._pointsPerHour ?? 0,
                    entranceZ);
                DefaultDwellingValues[spec.name] = def;
            }

            var maxBeavers = Math.Max(1, (int)(def.MaxBeavers * MSettings.MaxBeaverMultiplier));
            Debug.Log($"ConfigurableHousing: {spec.name} max beavers: {def.MaxBeavers} x {MSettings.MaxBeaverMultiplier} = {maxBeavers}");
            spec._maxBeavers = maxBeavers;
            
            if (hasEntrance)
            {
                var coor = entrance!.Value.Coordinates;
                blockSpec!._entrance._coordinates = new(coor.x, coor.y, MSettings.MoveEntranceFloor ? 0 : def.EntranceZ);
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
        }
    }

}
