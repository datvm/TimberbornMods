namespace ScenarioEditor.Services;

public class MultiStartingLocationsService(
    StartingLocationService startingLocationService,
    EntityComponentRegistry entityComponentRegistry,
    StartingBuildingSpawner startingBuildingSpawner,
    ISceneLoader sceneLoader
) : ILoadableSingleton, IUnloadableSingleton
{
    
    public static MultiStartingLocationsService? Instance { get; private set; }

    public ISceneLoader SceneLoader { get; } = sceneLoader;
    public StartingLocationSpec? ProcessingStartingLocation { get; private set; }

    readonly List<StartingLocationPair> startingBuildingsSpecs = [];
    public IReadOnlyList<StartingLocationPair> StartingBuildingsSpecs => startingBuildingsSpecs;

    public void Load()
    {
        Instance = this;
    }

    public void Unload()
    {
        Instance = null;
    }

    public bool HasStartingLocation() => startingLocationService.HasStartingLocation();

    public IEnumerable<Placement> GetPlacements() 
        => GetStartingLocations()
            .Select(q => q.GetComponentFast<BlockObject>().Placement);

    public IEnumerable<StartingLocationSpec> GetStartingLocations() 
        => entityComponentRegistry.GetEnabled<StartingLocationSpec>();

    public void PlaceStartingBuilding(StartingLocationSpec spec)
    {
        ProcessingStartingLocation = spec;

        var customStart = spec.GetComponentFast<CustomStartComponent>();
        startingBuildingSpawner.PlaceStartingBuilding(spec.GetPlacement());
        startingBuildingsSpecs.Add(new(startingBuildingSpawner.StartingBuildingSpec, customStart.Parameters));

        ProcessingStartingLocation = null;
    }

}

public readonly record struct StartingLocationPair(BuildingSpec BuildingSpec, CustomStartParameters CustomStart);