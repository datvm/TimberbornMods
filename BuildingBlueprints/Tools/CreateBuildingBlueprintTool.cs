namespace BuildingBlueprints.Tools;

[BindSingleton]
public class CreateBuildingBlueprintTool(
    ILoc t,
    AreaBlockObjectPickerFactory areaBlockObjectPickerFactory,
    BlockObjectSelectionDrawerFactory blockObjectSelectionDrawerFactory,
    CursorService cursorService,
    InputService inputService,
    ISpecService specService,
    IContainer container,
    BuildingBlueprintsService buildingBlueprintsService
) : ITool, IToolDescriptor, ILoadableSingleton, IInputProcessor, IConstructionModeEnabler
{
#nullable disable
    ToolDescription toolDescription;
    AreaBlockObjectPicker picker;
    BlockObjectSelectionDrawer highlighter;
    HotkeyEntry duplicateSettingEntry;
#nullable enable

    bool copySettings = true;

    public void Load()
    {
        var hotkeySection = container.GetInstance<HotkeyToolDescriptionSection>();
        duplicateSettingEntry = hotkeySection.AddEntry(DuplicationInputProcessor.DuplicateSettingsKey);
        UpdateHotkeys();

        toolDescription = new ToolDescription.Builder(t.T("LV.BB.BlueprintCreate"))
            .AddSection(t.T("LV.BB.BlueprintCreateDesc"))
            .AddPrioritizedSection(t.T("LV.BB.BlueprintCreateTip"))
            .AddSection(hotkeySection.Root)
            .Build();

        picker = areaBlockObjectPickerFactory.CreatePickingUpwards();

        var colorSpec = specService.GetSingleSpec<BuilderPriorityToolSpec>();
        highlighter = blockObjectSelectionDrawerFactory.Create(colorSpec.PriorityActionColor, colorSpec.PriorityTileColor, colorSpec.PrioritySideColor);
    }

    void UpdateHotkeys()
    {
        duplicateSettingEntry.Text = t.T("LV.BB.BlueprintCreateCopySettings", t.TYesNo(copySettings));
    }

    public ToolDescription DescribeTool() => toolDescription;

    public void Enter()
    {
        inputService.AddInputProcessor(this);
        cursorService.SetCursor("BuildingBlueprintCursor");
    }

    public void Exit()
    {
        cursorService.ResetCursor();
        picker.Reset();
        ClearHighlights();
        inputService.RemoveInputProcessor(this);
    }

    public bool ProcessInput() =>
        ProcessCopyKey() 
        || picker.PickBlockObjects<PlaceableBlockObjectSpec>(PreviewCallback, ActionCallback, ShowNoneCallback);

    bool ProcessCopyKey()
    {
        if (inputService.IsKeyDown(DuplicationInputProcessor.DuplicateSettingsKey))
        {
            copySettings = !copySettings;
            UpdateHotkeys();
            return true;
        }

        return false;
    }

    void PreviewCallback(IEnumerable<BlockObject> blockObjects, Vector3Int start, Vector3Int end, bool selectionStarted, bool selectingArea)
    {
        if (!selectingArea)
        {
            ClearHighlights();
            return;
        }

        var area = BuildingBlueprintsService.FromAreaSelection(start, end);
        var filtered = blockObjects.Where(b => buildingBlueprintsService.FilterSelection(b, area));
        highlighter.Draw(filtered, start, end, true);
    }

    void ActionCallback(IEnumerable<BlockObject> blockObjects, Vector3Int start, Vector3Int end, bool selectionStarted, bool selectingArea)
    {
        ClearHighlights();

        if (!selectingArea) { return; }

        var area = BuildingBlueprintsService.FromAreaSelection(start, end);
        var baseZ = Math.Min(start.z, end.z);

        var bos = blockObjects.Where(b => buildingBlueprintsService.FilterSelection(b, area));
        var info = BlueprintSelectionInfo.CreateFromSelection(t.T("LV.BB.DefaultName"), [..bos], area, baseZ, copySettings);
        if (!info.HasAnyBuilding) { return; }

        var diag = container.GetInstance<BlueprintCreationDialog>();
        diag.Show(info);
    }

    void ShowNoneCallback() => ClearHighlights();
    void ClearHighlights() => highlighter.StopDrawing();
}