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
    BlockObjectPreviewPicker blockObjectPreviewPicker,
    BlueprintBuildingSettingsService blueprintBuildingSettingsService
) : IInputProcessor, ILoadableSingleton
{
    static readonly Vector3Int PlaceholderCoordinates = new(-1, -1, -1);

    PreviewShowerSpec spec = null!;

    ParsedBlueprintInfo? blueprintInfo;
    ParsedBlueprintBuildingPlacement? firstBuilding;
    TaskCompletionSource<BuildingBlueprintPlacement?>? tcs;
    PreviewEnumerator? enumerator;
    readonly Dictionary<Preview, ParsedBlueprintBuildingPlacement> previewPairs = [];
    Vector3Int coordinates;
    Vector3Int prevCoordinates;
    bool lastPositionValid;
    bool shouldShow;
    bool firstCoordinate;


    public event Action OnBlueprintPlacementSettingsChanged = null!;
    public Orientation BlueprintOrientation { get; private set; } = Orientation.Cw0;
    public bool BlueprintFlip { get; private set; } = false;
    public bool IgnoreSettings { get; private set; }

    public void Load()
    {
        spec = specService.GetSingleSpec<PreviewShowerSpec>();
    }

    public async Task<BuildingBlueprintPlacement?> PlaceAsync(ParsedBlueprintInfo bp)
    {
        CleanUp();
        firstCoordinate = true;

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

        if (ProcessClick()) { return true; }

        var moved = ProcessCursorLocation();
        var rotated = ProcessRotationInput();
        var flipped = ProcessFlipInput();
        var ignoreCopyChanged = ProcessIgnoreCopy();

        var nonMovementChanged = rotated || flipped;

        if (nonMovementChanged || moved)
        {
            PositionPreviews();

            if (shouldShow)
            {
                _ = ValidatePreviewsAsync();
            }
        }

        var settingsChanged = nonMovementChanged || ignoreCopyChanged;
        if (settingsChanged)
        {
            OnBlueprintPlacementSettingsChanged();
        }

        return settingsChanged;
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
        tcs.TrySetResult(new(coordinates, BlueprintOrientation));
        return true;
    }

    async Task PlaceAsync()
    {
        var buildings = await placementValidator.BuildPreviewsAsync(enumerator!.AllPreviews);

        var groupId = blueprintGroupService.GetNextGroup();
        var copy = !IgnoreSettings;
        foreach (var (preview, b) in buildings)
        {
            var bpComp = b.GetComponent<BuildingBlueprintComponent>();
            if (bpComp)
            {
                bpComp.AssignToGroup(groupId);
            }

            if (copy)
            {
                var info = previewPairs[preview];
                blueprintBuildingSettingsService.ApplySettings(b, info.Settings);
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
            RotateOrientation(firstBuilding.Orientation, BlueprintOrientation),
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

        if (firstCoordinate)
        {
            firstCoordinate = false;
            shouldShow = false;
        }

        return true;
    }

    bool ProcessRotationInput()
    {
        bool rotated = false;
        if (inputService.IsKeyDown(BlockObjectPlacementPanel.RotateClockwiseKey))
        {
            BlueprintOrientation = BlueprintOrientation.RotateClockwise();
            rotated = true;
        }
        else if (inputService.IsKeyDown(BlockObjectPlacementPanel.RotateCounterclockwiseKey))
        {
            BlueprintOrientation = BlueprintOrientation.RotateCounterclockwise();
            rotated = true;
        }

        return rotated;
    }

    bool ProcessFlipInput()
    {
        if (inputService.IsKeyDown(BlockObjectPlacementPanel.FlipKey))
        {
            BlueprintFlip = !BlueprintFlip;
            return true;
        }

        return false;
    }

    bool ProcessIgnoreCopy()
    {
        if (inputService.IsKeyDown(DuplicationInputProcessor.DuplicateSettingsKey))
        {
            IgnoreSettings = !IgnoreSettings;
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
        previewPairs.Clear();
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
        previewPairs.Clear();
        foreach (var b in blueprintInfo.Buildings)
        {
            var name = b.Building.TemplateName;
            var preview = enumerator.GetNext(name);

            var placement = GetBuildingPlacement(b);
            preview.Reposition(placement);

            previewPairs.Add(preview, b);
        }

        previewShower.ShowBuildablePreviews(enumerator.AllPreviews, out _);
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
        var (b, localPos, localRot, localFlip, _) = buildingPlacement;

        if (BlueprintFlip)
        {
            var bos = b.BlockObjectSpec!;
            var w = bos.Size.x;

            localPos = TransformFlipX(localPos, localRot, w);
            localRot = MirrorOrientation(localRot);
            localFlip = Flip(localFlip, bos);
        }

        localPos = BlueprintOrientation.Transform(localPos);
        localRot = RotateOrientation(localRot, BlueprintOrientation);

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
