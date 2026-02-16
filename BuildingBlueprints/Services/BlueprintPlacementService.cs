namespace BuildingBlueprints.Services;

public readonly record struct BuildingBlueprintPlacement(Vector3Int Coordinates, Orientation Orientation);

[BindSingleton]
public class BlueprintPlacementService(
    CursorCoordinatesPicker cursorCoordinatesPicker,
    BlueprintPreviewRepository previewRepository,
    InputService inputService,
    PreviewShower previewShower
) : IInputProcessor
{
    static readonly Vector3Int PlaceholderCoordinates = new(-1, -1, -1);

    ParsedBlueprintInfo? blueprintInfo;
    TaskCompletionSource<BuildingBlueprintPlacement?>? tcs;
    PreviewEnumerator? enumerator;
    Orientation orientation = Orientation.Cw0;
    Vector3Int coordinates;
    Vector3Int prevCoordinates;
    bool lastPositionValid;

    public async Task<BuildingBlueprintPlacement?> PickAreaAsync(ParsedBlueprintInfo bp)
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
        }

        return ProcessClick() || rotated;
    }

    bool ProcessClick()
    {
        if (!inputService.MainMouseButtonDown || tcs is null) { return false; }

        if (!lastPositionValid)
        {

            return true;
        }

        tcs.TrySetResult(new(coordinates, orientation));
        return true;
    }

    bool ProcessCursorLocation()
    {
        var pos = cursorCoordinatesPicker.PickCoordinates(false);

        if (pos is null)
        {
            if (prevCoordinates == PlaceholderCoordinates) { return false; }
            prevCoordinates = coordinates = PlaceholderCoordinates;
            return true;
        }

        if (pos.Value.TileCoordinates == prevCoordinates) { return false; }

        prevCoordinates = coordinates = pos.Value.TileCoordinates;
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

        tcs?.TrySetResult(null);
        tcs = null;

        if (enumerator is not null)
        {
            previewShower.HidePreviews(enumerator.AllPreviews);
            enumerator = null;
        }

        prevCoordinates = PlaceholderCoordinates;
        blueprintInfo = null;
    }

    void InitializePreviews(ParsedBlueprintInfo bp)
    {
        enumerator = previewRepository.GetPreviews(bp.TemplatesAndCount);
    }

    void PositionPreviews()
    {
        if (enumerator is null || blueprintInfo is null) { return; }

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

    static Orientation RotateBy(Orientation src, Orientation by) => by switch
    {
        Orientation.Cw0 => src,
        Orientation.Cw90 => src.RotateClockwise(),
        Orientation.Cw180 => src.RotateClockwise().RotateClockwise(),
        Orientation.Cw270 => src.RotateCounterclockwise(),
        _ => throw new ArgumentOutOfRangeException()
    };

}
