global using Timberborn.BlueprintSystem;
using Timberborn.PrefabGroupSystem;
using Timberborn.TimbermeshMaterials;

namespace ConfigurableHousing.Patches;

[HarmonyPatch]
public static class FactionsPatches
{
    static readonly ImmutableArray<string> FolktailsHousesPrefabs =
    [
        "Buildings/Housing/DoubleLodge/DoubleLodge.Folktails",
        "Buildings/Housing/Lodge/Lodge.Folktails",
        "Buildings/Housing/MiniLodge/MiniLodge.Folktails",
        "Buildings/Housing/TripleLodge/TripleLodge.Folktails",
    ];

    static readonly ImmutableArray<string> FolktailsHousesMaterials =
    [
        "materials/uberatlas/materials/folktails/BaseWood_Brown.Folktails",
        "materials/uberatlas/materials/folktails/WindowsAtlas.Folktails",
        "materials/uberatlas/materials/folktails/PlasteredWood_White.Folktails",
        "materials/uberatlas/materials/folktails/IrregularPlanks_Mossy.Folktails",
        "materials/uberatlas/materials/folktails/BaseWood_LightBrown.Folktails",
    ];

    static readonly ImmutableArray<string> IronTeethPrefabs =
    [
        "Buildings/Housing/Barrack/Barrack.IronTeeth",
        "Buildings/Housing/LargeBarrack/LargeBarrack.IronTeeth",
        "Buildings/Housing/LargeRowhouse/LargeRowhouse.IronTeeth",
        "Buildings/Housing/Rowhouse/Rowhouse.IronTeeth",
    ];

    static readonly ImmutableArray<string> IronTeethMaterials =
    [
        "materials/uberatlas/materials/ironteeth/BaseWood_DarkBrown.IronTeeth",
        "materials/uberatlas/materials/ironteeth/WindowsAtlas.IronTeeth",
        "materials/uberatlas/materials/ironteeth/PlasteredWood_Orange.IronTeeth",
        "materials/uberatlas/materials/ironteeth/IrregularPlanks_DarkBrown.IronTeeth",
        "materials/uberatlas/materials/ironteeth/IrregularPlanks_Grey.IronTeeth",
    ];

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
        if (!MSettings.AddOtherFaction) { return; }

        switch (__result)
        {
            case PrefabGroupSpec prefGrp:
                {
                    var replace = AppendValue("Buildings.IronTeeth", "Buildings/Paths/Path/Path.IronTeeth", FolktailsHousesPrefabs, prefGrp);
                    if (replace is not null)
                    {
                        Debug.Log("Added IronTeeth faction buildings");
                        __result = replace;
                    }
                    else
                    {
                        replace = AppendValue("Buildings.Folktails", "Buildings/Paths/Path/Path.Folktails", IronTeethPrefabs, prefGrp);
                        if (replace is not null)
                        {
                            Debug.Log("Added Folktails faction buildings");
                            __result = replace;
                        }
                    }

                }

                break;
            case MaterialGroupSpec matGrp:
                {
                    var replace = AppendValue("IronTeeth", "Materials/UberAtlas/Materials/IronTeeth/BaseMetal.IronTeeth", FolktailsHousesMaterials, matGrp);
                    if (replace is not null)
                    {
                        Debug.Log("Added IronTeeth faction materials");
                        __result = replace;
                    }
                    else
                    {
                        replace = AppendValue("Folktails", "Materials/UberAtlas/Materials/Folktails/BaseMetal.Folktails", IronTeethMaterials, matGrp);
                        if (replace is not null)
                        {
                            Debug.Log("Added Folktails faction materials");
                            __result = replace;
                        }
                    }
                }

                break;
        }
    }

}
