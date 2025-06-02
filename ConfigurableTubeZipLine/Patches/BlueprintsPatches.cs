namespace ConfigurableTubeZipLine.Patches;

[HarmonyPatch]
public static class BlueprintsPatches
{
    #region Definitions

    const string ZiporterPrefabPath = "Buildings/Paths/Ziporter/Ziporter.Folktails";

    static readonly ImmutableArray<string> ZiporterRemovals =
    [
        "Buildings/Paths/ZiplineBeam/ZiplineBeam.Folktails",
        "Buildings/Paths/ZiplinePylon/ZiplinePylon.Folktails",
    ];

    static readonly ImmutableArray<string> ZiplinePrefabs =
    [
        ..ZiporterRemovals,
        "Buildings/Paths/ZiplineStation/ZiplineStation.Folktails",
    ];

    static readonly ImmutableArray<string> ZiplineMaterials =
    [
        "materials/uberatlas/materials/folktails/BaseWood_Brown.Folktails",
        "materials/uberatlas/materials/folktails/BaseMetal.Folktails",
        "materials/uberatlas/materials/folktails/WindowsAtlas.Folktails",
        "materials/uberatlas/materials/folktails/IrregularPlanks_White.Folktails",
        "materials/uberatlas/materials/folktails/IrregularPlanks_LightBrown.Folktails",
        "materials/uberatlas/materials/folktails/IrregularPlanks_Brown.Folktails",
        "materials/uberatlas/materials/folktails/BaseWood_LightBrown.Folktails",
        "materials/uberatlas/materials/folktails/BaseWood_White.Folktails",
        "materials/uberatlas/materials/folktails/Details.Folktails",
    ];

    static readonly ImmutableArray<string> TubewayPrefabs =
    [
        "Buildings/Paths/VerticalTubeway/VerticalTubeway.IronTeeth",
        "Buildings/Paths/Tubeway/Tubeway.IronTeeth",
        "Buildings/Paths/TubewayStation/TubewayStation.IronTeeth",
    ];

    static readonly ImmutableArray<string> TubewayMaterials =
    [
        "materials/uberatlas/materials/ironteeth/BaseWood_Indigo.IronTeeth",
        "materials/uberatlas/materials/ironteeth/BaseMetal.IronTeeth",
        "materials/uberatlas/materials/ironteeth/WindowsAtlas.IronTeeth",
        "materials/uberatlas/materials/ironteeth/BaseWood_DarkBrown.IronTeeth",
        "materials/uberatlas/materials/ironteeth/BaseWood_Grey.IronTeeth",
        "materials/uberatlas/materials/ironteeth/RoofPlanks.IronTeeth",
    ];

    #endregion

    static ImmutableArray<string>? AppendValue(string expectedId, string actualId, string expectedPath, IEnumerable<string> appending, IEnumerable<string> paths)
    {
        if (actualId != expectedId || !paths.Contains(expectedPath)) { return null; }
        return [.. paths.Concat(appending).Distinct()];
    }

    static PrefabGroupSpec? AppendValue(string expectedId, string expectedPath, IEnumerable<string> appending, PrefabGroupSpec spec)
    {
        var values = AppendValue(expectedId, spec.Id, expectedPath, appending, spec.Paths);
        return values is null ? null : spec with { Paths = values.Value };
    }

    static MaterialGroupSpec? AppendValue(string expectedId, string expectedPath, IEnumerable<string> appending, MaterialGroupSpec spec)
    {
        var values = AppendValue(expectedId, spec.Id, expectedPath, appending, spec.Paths);
        return values is null ? null : spec with { Paths = values.Value };
    }

    [HarmonyPostfix, HarmonyPatch(typeof(BasicDeserializer), nameof(BasicDeserializer.Deserialize))]
    public static void Deserialize(ref object __result)
    {
        if (MSettings.ZiplineForIronTeeth)
        {
            switch (__result)
            {
                case PrefabGroupSpec prefGrp:
                    {
                        IEnumerable<string> addingPrefabs = ZiplinePrefabs;
                        if (ModStarter.ZiporterInstalled)
                        {
                            addingPrefabs = addingPrefabs.Append(ZiporterPrefabPath);
                        }

                        var replace = AppendValue("Buildings.IronTeeth", "Buildings/Paths/Path/Path.IronTeeth", addingPrefabs, prefGrp);
                        if (replace is not null) { __result = replace; }
                    }

                    break;
                case MaterialGroupSpec matGrp:
                    {
                        var replace = AppendValue("IronTeeth", "Materials/UberAtlas/Materials/IronTeeth/BaseMetal.IronTeeth", ZiplineMaterials, matGrp);
                        if (replace is not null) { __result = replace; }
                    }

                    break;
            }
        }

        if (MSettings.TubewayForFolktails)
        {
            switch (__result)
            {
                case PrefabGroupSpec prefGrp:
                    {
                        var replace = AppendValue("Buildings.Folktails", "Buildings/Paths/Path/Path.Folktails", TubewayPrefabs, prefGrp);
                        if (replace is not null) { __result = replace; }
                    }

                    break;
                case MaterialGroupSpec matGrp:
                    {
                        var replace = AppendValue("Folktails", "Materials/UberAtlas/Materials/Folktails/BaseMetal.Folktails", TubewayMaterials, matGrp);
                        if (replace is not null) { __result = replace; }
                    }

                    break;
            }
        }
    }


}
