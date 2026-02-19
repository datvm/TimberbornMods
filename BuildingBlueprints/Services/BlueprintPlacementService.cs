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
    Orientation blueprintOrientation = Orientation.Cw0;
    bool blueprintFlip = false;
    Vector2Int blueprintSize;
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
        var flipped = ProcessFlipInput();

        if (moved || rotated || flipped)
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
        tcs.TrySetResult(new(coordinates, blueprintOrientation));
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
            RotateOrientation(firstBuilding.Orientation, blueprintOrientation),
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
            blueprintOrientation = blueprintOrientation.RotateClockwise();
            rotated = true;
        }
        else if (inputService.IsKeyDown(BlockObjectPlacementPanel.RotateCounterclockwiseKey))
        {
            blueprintOrientation = blueprintOrientation.RotateCounterclockwise();
            rotated = true;
        }

        return rotated;
    }

    bool ProcessFlipInput()
    {
        if (inputService.IsKeyDown(BlockObjectPlacementPanel.FlipKey))
        {
            blueprintFlip = !blueprintFlip;
            return true;
        }

        return false;
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
        blueprintSize = blueprintInfo.Size;
        foreach (var b in blueprintInfo.Buildings)
        {
            var name = b.Building.TemplateName;
            var preview = enumerator.GetNext(name);

            var placement = GetBuildingPlacement(b);
            preview.Reposition(placement);
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

    Placement GetBuildingPlacement(in ParsedBlueprintBuildingPlacement buildingPlacement)
    {
        var (b, localPos, localRot, localFlip) = buildingPlacement;

        if (blueprintFlip)
        {
            var bos = b.BlockObjectSpec!;
            var w = bos.Size.x;

            localPos = TransformFlipX(localPos, localRot, w);
            localRot = MirrorOrientation(localRot);
            localFlip = Flip(localFlip, bos);
        }

        localPos = blueprintOrientation.Transform(localPos);
        localRot = RotateOrientation(localRot, blueprintOrientation);

        var globalPos = coordinates + localPos;
        return new(globalPos, localRot, localFlip);
    }

    // Note: AI code, it works but I have no idea how.
    static Vector3Int TransformFlipX(Vector3Int p, Orientation orientation, int w) => orientation switch
    {
        Orientation.Cw0 => new Vector3Int(-p.x - (w - 1), p.y, p.z),
        Orientation.Cw90 => new Vector3Int(-p.x, p.y - (w - 1), p.z),
        Orientation.Cw180 => new Vector3Int(-p.x + (w - 1), p.y, p.z),
        Orientation.Cw270 => new Vector3Int(-p.x, p.y + (w - 1), p.z),
        _ => throw new ArgumentOutOfRangeException()
    };

    static Orientation RotateOrientation(Orientation src, Orientation by) => by switch
    {
        Orientation.Cw0 => src,
        Orientation.Cw90 => src.RotateClockwise(),
        Orientation.Cw180 => src.RotateClockwise().RotateClockwise(),
        Orientation.Cw270 => src.RotateCounterclockwise(),
        _ => throw new ArgumentOutOfRangeException()
    };

    static Orientation MirrorOrientation(Orientation src) => src switch
    {
        Orientation.Cw90 => Orientation.Cw270,
        Orientation.Cw270 => Orientation.Cw90,
        _ => src,
    };

    static FlipMode Flip(FlipMode localFlip, BlockObjectSpec bos) 
        => (localFlip.IsFlipped || bos.Flippable) ? localFlip.Flip() : localFlip;

}
