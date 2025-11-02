
namespace TImprove4UX.Services;

public class AlternateDeleteObjectTool(
    InputService inputService,
    AreaBlockObjectAndTerrainPicker areaBlockObjectAndTerrainPicker,
    EntityService entityService,
    BlockObjectSelectionDrawerFactory blockObjectSelectionDrawerFactory,
    CursorService cursorService,
    ILoc loc,
    BlockObjectModelBlockadeIgnorer blockObjectModelBlockadeIgnorer,
    ISpecService specService,
    ILevelVisibilityService levelVisibilityService,
    DialogBoxShower dialogBoxShower,
    RecoverableGoodElementFactory recoverableGoodElementFactory,
    RecoverableGoodTooltip recoverableGoodTooltip,
    TerrainDestroyer terrainDestroyer,
    TerrainHighlightingService terrainHighlightingService,
    IUndoRegistry undoRegistry,
    MSettings s,
    ToolService toolManager
) : BuildingDeconstructionTool(inputService, areaBlockObjectAndTerrainPicker, entityService, blockObjectSelectionDrawerFactory, cursorService, loc, blockObjectModelBlockadeIgnorer, specService, levelVisibilityService, dialogBoxShower, recoverableGoodElementFactory, recoverableGoodTooltip, terrainDestroyer, terrainHighlightingService, undoRegistry),
    ILoadableSingleton, IUnloadableSingleton, IInputProcessor
{
    const string AlternateHotkeyId = "AlternateClickableAction";

    readonly InputService inputService = inputService;
    BlockObjectTool? tool;
    bool exitByThisTool;

    public static AlternateDeleteObjectTool? Instance { get; private set; }

    public string? ExpectedTemplateName
    {
        get
        {
            if (tool is null || s.ShiftToDeleteAll.Value)
            {
                return null;
            }

            return tool.Template.GetSpec<TemplateSpec>().TemplateName;
        }
    }

    void ILoadableSingleton.Load()
    {
        Instance = this;
        Load();
    }

    public bool InterceptToolInput(BlockObjectTool instance)
    {
        if (!s.ShiftToDelete.Value || !inputService.IsKeyDown(AlternateHotkeyId))
        {
            return false;
        }

        tool = instance;
        toolManager.SwitchTool(this);
        return true;
    }

    public override bool IsBlockObjectValid(BlockObject blockObject)
    {
        if (!base.IsBlockObjectValid(blockObject)) { return false; }

        var expectedPrefab = ExpectedTemplateName;
        return expectedPrefab is null || blockObject.GetComponent<TemplateSpec>().TemplateName == ExpectedTemplateName;
    }

    public override void Enter()
    {
        base.Enter();
        tool?.Exit();
    }

    public override void Exit()
    {
        base.Exit();

        var prev = tool;
        if (prev is null) { return; }

        tool = null;

        if (exitByThisTool)
        {
            toolManager.SwitchTool(prev);
        }
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
            exitByThisTool = true;
            Exit();
            exitByThisTool = false;
            return true;
        }

        return ProcessInput();
    }

    public void Unload()
    {
        Instance = null;
    }

}
