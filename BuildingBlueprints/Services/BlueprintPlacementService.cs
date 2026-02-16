namespace BuildingBlueprints.Services;

public readonly record struct BuildingBlueprintPlacement(Vector3Int Coordinates, Orientation Orientation);


[BindSingleton]
public class BlueprintPlacementService(
    BlueprintPreviewRepository previewRepository,
    InputService inputService,
    PreviewShower previewShower,
    BlueprintPlacementValidator placementValidator,
    Highlighter highlighter,
    ISpecService specService,
    UISoundController uiSoundController,
    BlueprintGroupService blueprintGroupService,
    CameraService cameraService,
    BlockObjectPreviewPicker blockObjectPreviewPicker
) : IInputProcessor, ILoadableSingleton
{
    static readonly Vector3Int PlaceholderCoordinates = new(-1, -1, -1);

    PreviewShowerSpec spec = null!;

    ParsedBlueprintInfo? blueprintInfo;
    ParsedBlueprintBuildingPlacement? firstBuilding;
    TaskCompletionSource<BuildingBlueprintPlacement?>? tcs;
    PreviewEnumerator? enumerator;
    Orientation orientation = Orientation.Cw0;
    Vector3Int coordinates;
    Vector3Int prevCoordinates;
    bool lastPositionValid;
    bool shouldShow;

    public void Load()
    {
        spec = specService.GetSingleSpec<PreviewShowerSpec>();
    }

    public async Task<BuildingBlueprintPlacement?> PlaceAsync(ParsedBlueprintInfo bp)
    {
        CleanUp();

        blueprintInfo = bp;        
        var t = tcs = new();
        InitializePreviews(bp);

        inputService.AddInputProcessor(this);
        return await t.Task;
    }

    public void Stop()
    {
        CleanUp();
    }

    public bool ProcessInput()
    {
        if (enumerator is null) { return false; }

        var moved = ProcessCursorLocation();
        var rotated = ProcessRotationInput();

        if (moved || rotated)
        {
            PositionPreviews();

            if (shouldShow)
            {
                _ = ValidatePreviewsAsync();
            }
        }

        return ProcessClick() || rotated;
    }

    bool ProcessClick()
    {
        if (!inputService.MainMouseButtonUp || tcs is null) { return false; }

        if (!lastPositionValid && !inputService.IsKeyHeld(AlternateClickable.AlternateClickableActionKey))
        {
            uiSoundController.PlayCantDoSound();
            return true;
        }

        _ = PlaceAsync();
        tcs.TrySetResult(new(coordinates, orientation));
        return true;
    }

    async Task PlaceAsync()
    {
        var buildings = await placementValidator.BuildPreviewsAsync(enumerator!.AllPreviews);

        var groupId = blueprintGroupService.GetNextGroup();
        foreach (var b in buildings)
        {
            var bpComp = b.GetComponent<BuildingBlueprintComponent>();
            if (bpComp)
            {
                bpComp.AssignToGroup(groupId);
            }
        }

        lastPositionValid = false;
        prevCoordinates = PlaceholderCoordinates;
    }

    bool ProcessCursorLocation()
    {
        var ray = cameraService.ScreenPointToRayInGridSpace(inputService.MousePosition);
        var coords = blockObjectPreviewPicker.CenteredPreviewCoordinates(
            firstBuilding!.Building.PlaceableSpec,
            RotateBy(firstBuilding.Orientation, orientation),
            ray);
        var pos = coords?.Coordinates;

        if (pos is null)
        {
            if (prevCoordinates == PlaceholderCoordinates) { return false; }
            prevCoordinates = coordinates = PlaceholderCoordinates;
            shouldShow = false;
            return true;
        }

        shouldShow = true;
        if (pos == prevCoordinates) { return false; }

        prevCoordinates = coordinates = pos.Value;
        return true;
    }

    bool ProcessRotationInput()
    {
        bool rotated = false;
        if (inputService.IsKeyDown(BlockObjectPlacementPanel.RotateClockwiseKey))
        {
            orientation = orientation.RotateClockwise();
            rotated = true;
        }
        else if (inputService.IsKeyDown(BlockObjectPlacementPanel.RotateCounterclockwiseKey))
        {
            orientation = orientation.RotateCounterclockwise();
            rotated = true;
        }

        return rotated;
    }

    void CleanUp()
    {
        inputService.RemoveInputProcessor(this);

        if (tcs is not null)
        {
            tcs.TrySetResult(null);
            tcs = null;
        }

        if (enumerator is not null)
        {
            previewShower.HidePreviews(enumerator.AllPreviews);
            enumerator = null;
        }
        highlighter.UnhighlightAllPrimary();

        prevCoordinates = PlaceholderCoordinates;
        coordinates = PlaceholderCoordinates;
        blueprintInfo = null;
        firstBuilding = null;
        lastPositionValid = false;
    }

    void InitializePreviews(ParsedBlueprintInfo bp)
    {
        enumerator = previewRepository.GetPreviews(bp.TemplatesAndCount);
        firstBuilding = bp.FirstBuilding;
    }

    void PositionPreviews()
    {
        lastPositionValid = false;
        if (enumerator is null || blueprintInfo is null) { return; }

        if (!shouldShow)
        {
            previewShower.HidePreviews(enumerator.AllPreviews);
            return;
        }

        enumerator.Reset();
        var anchor = coordinates;
        var bpRotation = orientation;

        foreach (var b in blueprintInfo.Buildings)
        {
            var name = b.Building.TemplateName;
            var preview = enumerator.GetNext(name);

            var pos = anchor + bpRotation.Transform(b.Coordinates);
            var ort = RotateBy(b.Orientation, bpRotation);

            preview.Reposition(new(pos, ort, b.Flip));
        }

        previewShower.ShowBuildablePreviews(enumerator.AllPreviews, out var warning);
    }

    async Task ValidatePreviewsAsync()
    {
        if (enumerator is null) { return; }

        var previews = enumerator.AllPreviews;
        var validated = await placementValidator.ValidatePreviewsAsync(previews);
        lastPositionValid = validated.Count == previews.Length;
        highlighter.UnhighlightAllPrimary();

        foreach (var p in previews)
        {
            var valid = lastPositionValid || validated.Contains(p);

            p.Show(valid ? PreviewState.BuildableNotLast : PreviewState.UnbuildableNotLast);
            highlighter.HighlightPrimary(p.BlockObject, valid ? spec.BuildablePreview : spec.UnbuildablePreview);
        }
    }

    static Orientation RotateBy(Orientation src, Orientation by) => by switch
    {
        Orientation.Cw0 => src,
        Orientation.Cw90 => src.RotateClockwise(),
        Orientation.Cw180 => src.RotateClockwise().RotateClockwise(),
        Orientation.Cw270 => src.RotateCounterclockwise(),
        _ => throw new ArgumentOutOfRangeException()
    };

}
