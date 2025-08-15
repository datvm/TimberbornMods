
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
) : BuildingDeconstructionTool(inputService, areaBlockObjectAndTerrainPicker, entityService, blockObjectSelectionDrawerFactory, cursorService, loc, blockObjectModelBlockadeIgnorer, specService, levelVisibilityService, dialogBoxShower, recoverableGoodElementFactory, recoverableGoodTooltip, terrainDestroyer, terrainHighlightingService),
    ILoadableSingleton, IUnloadableSingleton, IInputProcessor
{
    const string AlternateHotkeyId = "AlternateClickableAction";

    readonly InputService inputService = inputService;
    BlockObjectTool? tool;

    public static AlternateDeleteObjectTool? Instance { get; private set; }

    public string? ExpectedPrefab
    {
        get
        {
            if (tool is null || s.ShiftToDeleteAll.Value)
            {
                return null;
            }

            return tool.Prefab.GetComponentFast<PrefabSpec>().PrefabName;
        }
    }

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

    public override bool IsBlockObjectValid(BlockObject blockObject)
    {
        if (!base.IsBlockObjectValid(blockObject)) { return false; }

        var expectedPrefab = ExpectedPrefab;
        return expectedPrefab is null || blockObject.GetComponentFast<PrefabSpec>().PrefabName == ExpectedPrefab;
    }

    public override void Enter()
    {
        base.Enter();
        tool?.Exit();
    }

    public override void Exit()
    {
        base.Exit();
        tool?.Enter();
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

    bool IInputProcessor.ProcessInput()
    {
        if (!inputService.IsKeyHeld(AlternateHotkeyId))
        {
            Exit();
            return true;
        }

        return ProcessInput();
    }

    public void Unload()
    {
        Instance = null;
    }

}
