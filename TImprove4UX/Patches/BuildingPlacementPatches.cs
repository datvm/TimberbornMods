namespace TImprove4UX.Patches;

[HarmonyPatch]
public static class BuildingPlacementPatches
{

    static bool trackBuildings = false;
    static List<BlockObject> lastBuilt = [];

    [HarmonyPrefix, HarmonyPatch(typeof(BlockObjectTool), nameof(BlockObjectTool.Place))]
    public static void BeforePlacing()
    {
        if (UndoBuildingService.Instance is null) { return; }

        lastBuilt = [];
        trackBuildings = true;
    }

    [HarmonyPostfix, HarmonyPatch(typeof(BlockObjectTool), nameof(BlockObjectTool.Place))]
    public static void AfterPlacing()
    {
        if (!trackBuildings) { return; }

        trackBuildings = false;
        UndoBuildingService.Instance?.RegisterLastBuilt(lastBuilt);
        lastBuilt = [];
    }

    [HarmonyPostfix, HarmonyPatch(typeof(BlockObjectFactory), nameof(BlockObjectFactory.CreateUnfinished))]
    public static void OnBuildingCreated(BlockObject __result)
    {
        if (!trackBuildings || !__result) { return; }
        lastBuilt.Add(__result);
    }

}
