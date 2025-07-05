namespace MapResizer.UI;

public class MapResizerController(
    IOptionsBox optionsBox,
    ILoc t,
    MapSize mapSize,
    VisualElementInitializer veInit,
    PanelStack panelStack,
    MapResizeService mapResizer,
    DropdownItemsSetter dropdownItemsSetter,
    DialogBoxShower diagShower,
    BlockObjectResizeValidationService blockObjectResizeValidationService,
    EntitySelectionService entitySelectionService
) : ILoadableSingleton
{
    readonly GameOptionsBox? gameOptionsBox = optionsBox as GameOptionsBox;
    readonly MapEditorOptionsBox? mapEditorOptionsBox = optionsBox as MapEditorOptionsBox;

    public void Load()
    {
        var root = gameOptionsBox?._root
            ?? mapEditorOptionsBox?._root
            ?? throw new InvalidOperationException("No options box found.");

        InsertResizeButton(root);
    }

    NineSliceButton InsertResizeButton(VisualElement box)
    {
        var btnResume = box.Q("ResumeButton") ?? box.Q("Resume");

        var btn = box.AddMenuButton(t.T("LV.MRe.ResizeMap"), stretched: true, onClick: OnShowResizeDialogClicked);
        btn.InsertSelfAfter(btnResume);

        return btn;
    }

    void OnShowResizeDialogClicked()
    {
        // Close the game options box
        gameOptionsBox?.ResumeClicked(null);
        mapEditorOptionsBox?.OnResumeClicked(null);

        // Show the resize dialog
        var diag = new MapResizerDialog(mapSize, t, dropdownItemsSetter, veInit);
        diag.Show(
            veInit,
            panelStack,
            async () => await ResizeAsync(diag.ResizeValues));
    }

    async Task ResizeAsync(ResizeValues resizeValues)
    {
        if (!ValidateBlockObjects(resizeValues)) { return; }

        var saveRef = await mapResizer.PerformResizeAsync(resizeValues);

        diagShower.Create()
            .SetMessage("LV.MRe.OfferToLoad".T(t))
            .SetConfirmButton(() => LoadSave(saveRef))
            .SetDefaultCancelButton()
            .Show();
    }

    bool ValidateBlockObjects(in ResizeValues resizeValues)
    {
        var totalSize = resizeValues.TotalSize;
        var terrainSize = resizeValues.TerrainSize;

        var firstInvalidBlockObject = blockObjectResizeValidationService.GetFirstInvalidBlockObject(totalSize, terrainSize);
        if (!firstInvalidBlockObject) { return true; }

        diagShower.Create()
            .SetMessage("LV.MRe.ResizeInvalidObj".T(t))
            .SetConfirmButton(() => blockObjectResizeValidationService.DeleteInvalidBlockObjects(totalSize, terrainSize), "LV.MRe.AutoRemove".T(t))
            .SetDefaultCancelButton()
            .SetInfoButton(() => SelectInvalidObject(firstInvalidBlockObject), t.T("LV.MRe.SeeInvalidObj"))
            .Show();

        return false;
    }

    void SelectInvalidObject(BlockObject obj)
    {
        Debug.Log($"Invalid object: {obj}, at {obj.Coordinates}");
        entitySelectionService.SelectAndFocusOn(obj);
    }

    void LoadSave(ISaveReference saveRef)
    {
        mapResizer.LoadGame(saveRef);
    }

}
