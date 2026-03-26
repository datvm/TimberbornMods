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
    public const string NudgeKey = "NudgeBlueprint";
    public const string HighPerformanceBlueprintKey = "HighPerformanceBlueprint";

    const string HighPerformanceSaveKey = $"{nameof(BuildingBlueprints)}.Settings.{nameof(HighPerformanceMode)}";

    public static readonly ImmutableArray<string> CameraMoveKeys = [CameraMovementInput.MoveCameraUpKey, CameraMovementInput.MoveCameraLeftKey, CameraMovementInput.MoveCameraDownKey, CameraMovementInput.MoveCameraRightKey];
    public static readonly ImmutableArray<string> CameraRotationKeys = [CameraMovementInput.RotateCameraLeftKey, CameraMovementInput.RotateCameraRightKey];
    static readonly ImmutableArray<string> AllCameraKeys = [.. CameraMoveKeys, .. CameraRotationKeys];

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
    public bool IsNudging { get; private set; }
    public bool HighPerformanceMode { get; private set; }

    public void Load()
    {
        spec = specService.GetSingleSpec<PreviewShowerSpec>();
        HighPerformanceMode = PlayerPrefs.GetInt(HighPerformanceSaveKey, 0) == 1;
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

    bool processedInput;
    bool placementChanged;
    bool settingsChanged;
    public bool ProcessInput()
    {
        if (enumerator is null || tcs is null) { return false; }

        if (ProcessHighPerformanceToggle())
        {
            OnBlueprintPlacementSettingsChanged();
            return true;
        }

        processedInput = false;
        placementChanged = false;
        settingsChanged = false;

        ProcessNudgingToggle();
        if (ProcessPlacement()) { return true; }

        ProcessMovement();
        ProcessRotationInput();
        ProcessFlipInput();
        ProcessIgnoreCopy();

        if (placementChanged)
        {
            PositionPreviews();

            if (shouldShow)
            {
                _ = ValidatePreviewsAsync(false);
            }
        }

        if (settingsChanged)
        {
            OnBlueprintPlacementSettingsChanged();
        }

        return processedInput;
    }

    public void SetHighPerformanceMode(bool enabled)
    {
        HighPerformanceMode = enabled;
        PlayerPrefs.SetInt(HighPerformanceSaveKey, enabled ? 1 : 0);
        PlayerPrefs.Save();
    }

    bool ProcessPlacement()
    {
        var placeRequested = inputService.MainMouseButtonDown || inputService.WasConfirmPressedLastFrame;
        if (!placeRequested) { return false; }

        if (!lastPositionValid && !inputService.IsKeyHeld(AlternateClickable.AlternateClickableActionKey))
        {
            if (HighPerformanceMode)
            {
                _ = ValidatePreviewsAsync(true);
            }

            uiSoundController.PlayCantDoSound();
            return true;
        }

        _ = PlaceAsync();
        tcs!.TrySetResult(new(coordinates, BlueprintOrientation));
        return true;
    }

    async Task PlaceAsync()
    {
        var buildings = await placementValidator.BuildPreviewsAsync(enumerator!.AllPreviews);

        var groupId = blueprintGroupService.GetNextGroup();
        var copy = !IgnoreSettings;

        Dictionary<Guid, Guid> idMapping = [];

        foreach (var (preview, b) in buildings)
        {
            var info = previewPairs[preview];
            var originalId = info.OriginalId;

            var bpComp = b.GetComponent<BuildingBlueprintComponent>();
            if (bpComp)
            {
                bpComp.AssignToGroup(groupId, originalId);

                if (copy && originalId is not null)
                {
                    idMapping[originalId.Value] = bpComp.GetEntityId();
                }
            }
        }

        if (copy)
        {
            foreach (var (preview, b) in buildings)
            {
                var info = previewPairs[preview];
                blueprintBuildingSettingsService.ApplySettings(b, info.Settings, idMapping);
            }
        }

        lastPositionValid = false;
        prevCoordinates = PlaceholderCoordinates;
    }

    bool ProcessHighPerformanceToggle()
    {
        if (!inputService.IsKeyDown(HighPerformanceBlueprintKey)) { return false; }
        SetHighPerformanceMode(!HighPerformanceMode);
        return true;
    }

    void ProcessNudgingToggle()
    {
        if (!inputService.IsKeyDown(NudgeKey)) { return; }

        IsNudging = !IsNudging;
        processedInput = true;
        settingsChanged = true;
    }

    void ProcessMovement()
    {
        if (IsNudging)
        {
            ProcessKeyNudging();
        }
        else
        {
            ProcessCursorLocation();
        }
    }

    void ProcessKeyNudging()
    {
        // Disable camera key when nudging
        if (AllCameraKeys.Any(inputService.IsKeyHeld))
        {
            processedInput = true;
        }

        if (inputService.IsKeyDown(CameraMovementInput.RotateCameraLeftKey))
        {
            coordinates.z = Math.Max(coordinates.z - 1, 0);
            placementChanged = true;
        }

        if (inputService.IsKeyDown(CameraMovementInput.RotateCameraRightKey))
        {
            coordinates.z += 1;
            placementChanged = true;
        }

        var x = inputService.IsKeyDown(CameraMovementInput.MoveCameraLeftKey) ? -1
            : inputService.IsKeyDown(CameraMovementInput.MoveCameraRightKey) ? 1 : 0;
        var y = inputService.IsKeyDown(CameraMovementInput.MoveCameraUpKey) ? 1
            : inputService.IsKeyDown(CameraMovementInput.MoveCameraDownKey) ? -1 : 0;
        if (x == 0 && y == 0) { return; }

        var rawDirection = new Vector3(x, 0f, y).normalized;
        var worldDirection = Quaternion.AngleAxis(cameraService.HorizontalAngle, Vector3.up) * rawDirection;

        if (Math.Abs(worldDirection.x) > Math.Abs(worldDirection.z))
        {
            coordinates.x += Math.Sign(worldDirection.x);
        }
        else
        {
            coordinates.y += Math.Sign(worldDirection.z);
        }

        placementChanged = true;
    }

    void ProcessCursorLocation()
    {
        var ray = cameraService.ScreenPointToRayInGridSpace(inputService.MousePosition);
        var coords = blockObjectPreviewPicker.CenteredPreviewCoordinates(
            firstBuilding!.Building.PlaceableSpec,
            RotateOrientation(firstBuilding.Orientation, BlueprintOrientation),
            ray);
        var pos = coords?.Coordinates;

        if (pos is null)
        {
            if (prevCoordinates == PlaceholderCoordinates) { return; }
            prevCoordinates = coordinates = PlaceholderCoordinates;
            shouldShow = false;
            goto MOVED;
        }

        shouldShow = true;
        if (pos == prevCoordinates) { return; }

        prevCoordinates = coordinates = pos.Value;

        if (firstCoordinate)
        {
            firstCoordinate = false;
            shouldShow = false;
        }

    MOVED:
        placementChanged = true;
    }

    void ProcessRotationInput()
    {
        if (inputService.IsKeyDown(BlockObjectPlacementPanel.RotateClockwiseKey))
        {
            BlueprintOrientation = BlueprintOrientation.RotateClockwise();
            goto ROTATED;
        }
        else if (inputService.IsKeyDown(BlockObjectPlacementPanel.RotateCounterclockwiseKey))
        {
            BlueprintOrientation = BlueprintOrientation.RotateCounterclockwise();
            goto ROTATED;
        }

        return;
    ROTATED:
        placementChanged = true;
        processedInput = true;
        settingsChanged = true;
    }

    void ProcessFlipInput()
    {
        if (!inputService.IsKeyDown(BlockObjectPlacementPanel.FlipKey)) { return; }

        BlueprintFlip = !BlueprintFlip;
        placementChanged = true;
        processedInput = true;
        settingsChanged = true;
    }

    void ProcessIgnoreCopy()
    {
        if (!inputService.IsKeyDown(DuplicationInputProcessor.DuplicateSettingsKey)) { return; }
        IgnoreSettings = !IgnoreSettings;
        processedInput = true;
        settingsChanged = true;
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
        IsNudging = false;

        OnBlueprintPlacementSettingsChanged();
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

    async Task ValidatePreviewsAsync(bool force)
    {
        if (enumerator is null) { return; }

        var previews = enumerator.AllPreviews;
        var validated = await GetPreviewValidationAsync(previews, force);
        lastPositionValid = validated.Count == previews.Length;
        highlighter.UnhighlightAllPrimary();

        foreach (var p in previews)
        {
            var valid = lastPositionValid || validated.Contains(p);

            p.Show(valid ? PreviewState.BuildableNotLast : PreviewState.UnbuildableNotLast);
            highlighter.HighlightPrimary(p.BlockObject, valid ? spec.BuildablePreview : spec.UnbuildablePreview);
        }
    }

    async Task<HashSet<Preview>> GetPreviewValidationAsync(ImmutableArray<Preview> previews, bool force) => !force && HighPerformanceMode
        ? []
        : await placementValidator.ValidatePreviewsAsync(previews);

    Placement GetBuildingPlacement(in ParsedBlueprintBuildingPlacement buildingPlacement)
    {
        var (b, _, localPos, localRot, localFlip, _) = buildingPlacement;

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
