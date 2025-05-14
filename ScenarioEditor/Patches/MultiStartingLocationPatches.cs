namespace ScenarioEditor.Patches;

[HarmonyPatch]
public static class MultiStartingLocationPatches
{

    [HarmonyPrefix, HarmonyPatch(typeof(StartingLocationService), nameof(StartingLocationService.OnEntityInitializedEvent))]
    public static bool DontDeleteOtherStartingLocations() => false;

    [HarmonyPrefix, HarmonyPatch(typeof(StartingLocationThumbnailRenderingListener), nameof(StartingLocationThumbnailRenderingListener.PreThumbnailRendering))]
    public static bool DontHideStartingLocation() => false;

    [HarmonyPrefix, HarmonyPatch(typeof(StartingBuildingInitializer), nameof(StartingBuildingInitializer.Initialize))]
    public static bool InitializeStartingLocations(StartingBuildingInitializer __instance)
    {
        var instance = MultiStartingLocationsService.Instance!;

        var locations = instance.GetStartingLocations().ToImmutableArray();

        if (locations.Length > 0)
        {
            __instance.InitialPlacement = locations[0].GetPlacement();
        }

        foreach (var loc in locations)
        {
            instance.PlaceStartingBuilding(loc);
        }

        __instance.SetCamera();
        __instance._startingLocationService.DeleteStartingLocations();

        return false;
    }

    [HarmonyPrefix, HarmonyPatch(typeof(StartingGoodsProvider), nameof(StartingGoodsProvider.InitialGoods))]
    public static bool ModifyStartingInventory(ref IEnumerable<GoodAmount> __result)
    {
        var starting = MultiStartingLocationsService.Instance?.ProcessingStartingLocation;
        if (!starting) { return true; }

        var custom = starting.GetComponentFast<CustomStartComponent>();
        if (!custom || !custom.Parameters.Enabled) { return true; }

        __result = custom.InitialGoods;
        return false;
    }

    [HarmonyPrefix, HarmonyPatch(typeof(GameInitializer), nameof(GameInitializer.SpawnBeavers))]
    public static bool SpawnBeavers(GameInitializer __instance, ref GameInitializer.InitializationState __result)
    {
        var service = MultiStartingLocationsService.Instance;

        var startingBuildings = service?.StartingBuildingsSpecs;
        if (startingBuildings is null || startingBuildings.Count == 0) { return true; }

        var newGameMode = service!.SceneLoader.GetSceneParameters<GameSceneParameters>().NewGameConfiguration.NewGameMode;

        foreach (var (b, custom) in startingBuildings)
        {
            var access = b.GetComponentFast<BuildingAccessible>().Accessible.UnblockedSingleAccess;
            if (!access.HasValue) { continue; }

            var adult = newGameMode.StartingAdults;
            var children = newGameMode.StartingChildren;

            if (custom?.Enabled == true)
            {
                adult = custom.Adult;
                children = custom.Children;
            }

            __instance._startingBeaverInitializer.Initialize(access.Value,
                adult, newGameMode.AdultAgeProgress,
                children, newGameMode.ChildAgeProgress);
        }

        __result = GameInitializer.InitializationState.PostSpawnBeavers;
        return false;
    }

}
