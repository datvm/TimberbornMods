namespace BuildingHP.Services;

public class BuildingHPService(
    ITerrainPhysicsService terrainPhysicsService,
    EntityService entityService,
    TerrainDestroyer terrainDestroyer,
    BuildingMaterialDurabilityService durServ,
    BuildingHPRegistry buildingRegistry,
    BuildingRenovationService renovationService,
    BuildingRepairService buildingRepairService,
    RenovationRegistry renovationRegistry,
    MSettings settings
)
{

    readonly HashSet<BlockObject> destroyingBos = [];
    readonly HashSet<Vector3Int> destroyingTerrains = [];

    public BuildingMaterialDurabilityService BuildingMaterialDurabilityService { get; } = durServ;
    public BuildingHPRegistry BuildingHPRegistry { get; } = buildingRegistry;
    public BuildingRenovationService BuildingRenovationService { get; } = renovationService;
    public BuildingRepairService BuildingRepairService { get; } = buildingRepairService;
    public RenovationRegistry RenovationRegistry { get; } = renovationRegistry;
    public MSettings Settings { get; } = settings;

    public void DestroyBuilding(BlockObject building)
    {
        terrainPhysicsService.GetTerrainAndBlockObjectStack([building], destroyingTerrains, destroyingBos);
        destroyingBos.Add(building);

        foreach (var bo in destroyingBos)
        {
            entityService.Delete(bo);
        }

        foreach (var pos in destroyingTerrains)
        {
            terrainDestroyer.DestroyTerrain(pos);
        }

        destroyingBos.Clear();
        destroyingTerrains.Clear();
    }

}
