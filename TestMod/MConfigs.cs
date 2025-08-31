namespace TestMod;

[Context("Game")]
public class ModGameConfig : Configurator
{
    public override void Configure()
    {

    }
}

[HarmonyPatch]
public static class TestPatch
{

    [HarmonyPostfix, HarmonyPatch(typeof(PrefabGroupService), nameof(PrefabGroupService.Load))]
    public static void PatchPrefab(PrefabGroupService __instance)
    {
        foreach (var p in __instance.AllPrefabs)
        {
            var building = p.GetComponent<BuildingSpec>();
            if (!building || building.BuildingCost.Any(q => q.GoodId == "Log")) { continue; }

            building._buildingCost =
            [
                .. building.BuildingCost,
                new()
                {
                    _amount = 1000,
                    _goodId = "Log",
                },
                new()
                {
                    _amount = 1000,
                    _goodId = "Gear",
                },
            ];
        }
    }

}

