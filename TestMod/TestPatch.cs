global using Timberborn.EntitySystem;
using Timberborn.Buildings;

namespace TestMod;

[HarmonyPatch]
public static class TestPatch
{

    [HarmonyPostfix, HarmonyPatch(typeof(EntityPanel), nameof(EntityPanel.Show))]
    public static void AfterShow(EntityComponent entity)
    {
        Debug.Log($"{entity.name}");

        var lighting = entity.GetComponentFast<BuildingLighting>();
        if (lighting is null) { return; }

        Debug.Log("Changing lighting");
        lighting._materialColorer.EnableLighting(entity.GameObjectFast, 10f);
    }

}
