
namespace TImprove4UX.Services;

public class AlternateDeleteObjectTool(
    InputService inputService, AreaBlockObjectAndTerrainPicker areaBlockObjectAndTerrainPicker,
    EntityService entityService, BlockObjectSelectionDrawerFactory blockObjectSelectionDrawerFactory,
    CursorService cursorService, ILoc loc, BlockObjectModelBlockadeIgnorer blockObjectModelBlockadeIgnorer,
    ISpecService specService, ILevelVisibilityService levelVisibilityService, DialogBoxShower dialogBoxShower,
    RecoverableGoodElementFactory recoverableGoodElementFactory, RecoverableGoodTooltip recoverableGoodTooltip,
    TerrainDestroyer terrainDestroyer,
    TerrainHighlightingService terrainHighlightingService,
    MSettings s
) : BuildingDeconstructionTool(inputService, areaBlockObjectAndTerrainPicker, entityService, blockObjectSelectionDrawerFactory, cursorService, loc, blockObjectModelBlockadeIgnorer, specService, levelVisibilityService, dialogBoxShower, recoverableGoodElementFactory, recoverableGoodTooltip, terrainDestroyer, terrainHighlightingService), ILoadableSingleton, IUnloadableSingleton
{
    const string AlternateHotkeyId = "AlternateClickableAction";

    readonly InputService inputService = inputService;
    BlockObjectTool? tool;

    public static AlternateDeleteObjectTool? Instance { get; private set; }

    public void Load()
    {
        Instance = this;
        Initialize();
    }

    public bool InterceptToolInput(BlockObjectTool instance)
    {
        if (!s.ShiftToDelete.Value || !inputService.IsKeyDown(AlternateHotkeyId))
        {
            return false;
        }

        tool = instance;
        Enter();
        return true;
    }

    public void FilterObjectsToDeconstruct(ref IEnumerable<BlockObject> blockObjects)
    {
        if (s.ShiftToDeleteAll.Value || tool is null)
        {
            return;
        }

        var expectedPrefab = tool.Prefab.GetComponentFast<PrefabSpec>().PrefabName;
        blockObjects = blockObjects.Where(bo =>
        {
            if (!bo) { return false; }
            var actualPrefab = bo.GetComponentFast<PrefabSpec>().PrefabName;
            return expectedPrefab == actualPrefab;
        });
    }

    public override void Enter()
    {
        base.Enter();
        tool?.Exit();
    }

    public override void Exit()
    {
        base.Exit();
        tool = null;
    }

    public bool AltToolProcessInput()
    {
        if (!inputService.IsKeyHeld(AlternateHotkeyId))
        {
            Exit();
            return true;
        }

        return false;
    }

    public void Unload()
    {
        Instance = null;
    }

}
