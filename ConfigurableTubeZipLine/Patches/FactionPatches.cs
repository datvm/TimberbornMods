using Timberborn.BlueprintSystem;
using Timberborn.PrefabGroupSystem;
using Timberborn.TimbermeshMaterials;

namespace ConfigurableTubeZipLine.Patches;

[HarmonyPatch]
public static class FactionPatches
{
    static readonly ImmutableArray<string> ZiplinePrefabs =
    [
        "Buildings/Paths/ZiplineBeam/ZiplineBeam.Folktails",
        "Buildings/Paths/ZiplinePylon/ZiplinePylon.Folktails",
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
        "Buildings/Paths/SolidTubeway/SolidTubeway.IronTeeth",
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

    [HarmonyPostfix, HarmonyPatch(typeof(BasicDeserializer), nameof(BasicDeserializer.Deserialize))]
    public static void Deserialize(ref object __result)
    {
        if (MSettings.ZiplineForIronTeeth)
        {
            switch (__result)
            {
                case PrefabGroupSpec prefGrp:
                    if (prefGrp.Id == "Buildings.IronTeeth")
                    {
                        __result = prefGrp with { Paths = [.. prefGrp.Paths, .. ZiplinePrefabs] };
                    }
                    break;
                case MaterialGroupSpec matGrp:
                    if (matGrp.Id == "IronTeeth")
                    {
                        __result = matGrp with { Paths = [.. matGrp.Paths, .. ZiplineMaterials] };
                    }

                    break;
            }
        }

        if (MSettings.TubewayForFolktails)
        {
            switch (__result)
            {
                case PrefabGroupSpec prefGrp:
                    if (prefGrp.Id == "Buildings.Folktails")
                    {
                        __result = prefGrp with { Paths = [.. prefGrp.Paths, .. TubewayPrefabs] };
                    }
                    break;
                case MaterialGroupSpec matGrp:
                    if (matGrp.Id == "Folktails")
                    {
                        __result = matGrp with { Paths = [.. matGrp.Paths, .. TubewayMaterials] };
                    }
                    break;
            }
        }
    }

}
