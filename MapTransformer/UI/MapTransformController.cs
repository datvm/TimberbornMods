namespace MapTransformer.UI;

[BindSingleton(Contexts = BindAttributeContext.NonMenu)]
class MapTransformController(
    IOptionsBox optionsBox,
    ILoc t,
    MapSize mapSize,
    VisualElementInitializer veInit,
    PanelStack panelStack,
    MapTransformService mapTransformService,
    DropdownItemsSetter dropdownItemsSetter,
    DialogBoxShower diagShower,
    EntityRegistry entities,
    EntityService entityService,
    EntitySelectionService entitySelectionService
) : ILoadableSingleton
{
    readonly GameOptionsBox? gameOptionsBox = optionsBox as GameOptionsBox;
    readonly MapEditorOptionsBox? mapEditorOptionsBox = optionsBox as MapEditorOptionsBox;
    readonly bool isGame = optionsBox is GameOptionsBox;

    public void Load()
    {
        var root = gameOptionsBox?._root
            ?? mapEditorOptionsBox?._root
            ?? throw new InvalidOperationException("No options box found.");

        var btnResume = root.Q("ResumeButton") ?? root.Q("Resume");
        var btn = root.AddMenuButton(t.T("LV.MTr.TransformMap"), stretched: true, onClick: OnShowDialogClicked);
        btn.InsertSelfAfter(btnResume);
    }

    void OnShowDialogClicked()
    {
        gameOptionsBox?.ResumeClicked(null);
        mapEditorOptionsBox?.OnResumeClicked(null);

        var diag = new MapTransformDialog(mapSize, t, dropdownItemsSetter, veInit, isGame);
        diag.Show(veInit, panelStack, async () => await TransformAsync(diag.BuildTransform()));
    }

    async Task TransformAsync(MapTransform transform)
    {
        if (transform.ErrorLocKey() is { } errorKey)
        {
            diagShower.Create()
                .SetMessage(errorKey.T(t))
                .SetConfirmButton(TimberUiUtils.DoNothing, "Core.OK".T(t))
                .Show();
            return;
        }

        if (!ValidateBlockObjects(transform))
        {
            return;
        }

        var saveRef = await mapTransformService.PerformAsync(transform);
        diagShower.Create()
            .SetMessage("LV.MTr.OfferToLoad".T(t))
            .SetConfirmButton(() => mapTransformService.Load(saveRef))
            .SetDefaultCancelButton()
            .Show();
    }

    bool ValidateBlockObjects(in MapTransform transform)
    {
        var invalid = mapTransformService.GetInvalidBlockObjects(transform, entities).ToList();
        if (invalid.Count == 0)
        {
            return true;
        }

        var first = invalid[0];
        diagShower.Create()
            .SetMessage("LV.MTr.InvalidObj".T(t))
            .SetConfirmButton(() => mapTransformService.DeleteBlockObjects(invalid, entityService), "LV.MTr.AutoRemove".T(t))
            .SetDefaultCancelButton()
            .SetInfoButton(() => entitySelectionService.SelectAndFocusOn(first), t.T("LV.MTr.SeeInvalidObj"))
            .Show();

        return false;
    }
}
