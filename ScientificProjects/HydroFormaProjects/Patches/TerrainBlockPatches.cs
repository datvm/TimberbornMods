using System.Reflection.Emit;

namespace HydroFormaProjects.Patches;

public static class TerrainBlockPatches
{

    readonly static FieldInfo MaxSupportDistanceField = typeof(TerrainPhysicsValidator).Field(nameof(TerrainPhysicsValidator.MaxSupportDistance));
    readonly static FieldInfo MaxSupportDistanceDoubledField = typeof(TerrainPhysicsValidator).Field(nameof(TerrainPhysicsValidator.MaxSupportDistanceDoubled));

    readonly static FieldInfo ModSupportDistanceField = typeof(TerrainBlockUpgradeService).Field(nameof(TerrainBlockUpgradeService.MaxHangingTerrain));
    readonly static FieldInfo ModSupportDistanceDoubledField = typeof(TerrainBlockUpgradeService).Field(nameof(TerrainBlockUpgradeService.MaxHangingTerrainDoubled));

    public static Harmony PatchHangingTerrains(this Harmony harmony)
    {
        Dictionary<Type, HashSet<string>> methodsByTypes = new()
        {
            { 
                typeof(TerrainPhysicsPostLoader),
                [
                    nameof(TerrainPhysicsPostLoader.ValidateTerrain),
                    nameof(TerrainPhysicsPostLoader.Enqueue),
                ]
            },
            {
                typeof(TerrainPhysicsService),
                [
                    nameof(TerrainPhysicsService.GetCheckAreaCoordinates)
                ]
            },
            {
                typeof(TerrainPhysicsValidator),
                [
                    nameof(TerrainPhysicsValidator.AreCoordinatesInvalid),
                    nameof(TerrainPhysicsValidator.IsTerrainValid),
                    nameof(TerrainPhysicsValidator.ValidateCheckedArea),
                    nameof(TerrainPhysicsValidator.AddCoordinatesToSets),
                    nameof(TerrainPhysicsValidator.AddDataToSets)
                ]
            }
        };

        var transpiler = typeof(TerrainBlockPatches).Method(nameof(PatchMaxSupportDistance));

        foreach (var (type, names) in methodsByTypes)
        {
            var methods = type.GetMethods(AccessTools.all);

            foreach (var m in methods)
            {
                if (names.Contains(m.Name))
                {
                    harmony.Patch(m, transpiler: transpiler);
                }
            }
        }

        return harmony;
    }

    public static IEnumerable<CodeInstruction> PatchMaxSupportDistance(IEnumerable<CodeInstruction> instructions)
    {
        foreach (var ins in instructions)
        {
            var isMaxSupportDistance = ins.opcode == OpCodes.Ldsfld ? IsMaxSupportDistanceField(ins.operand as FieldInfo) : null;

            if (isMaxSupportDistance is null)
            {
                yield return ins;
            }
            else
            {
                yield return new CodeInstruction(
                    OpCodes.Ldsfld,
                    isMaxSupportDistance.Value ?
                        ModSupportDistanceField :
                        ModSupportDistanceDoubledField
                );
            }
        }
    }

    static bool? IsMaxSupportDistanceField(FieldInfo? field)
    {
        if (field is null) { return null; }

        return field == MaxSupportDistanceField ? true :
            (field == MaxSupportDistanceDoubledField ? false : null);
    }

}
