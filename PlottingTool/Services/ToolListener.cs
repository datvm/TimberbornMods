namespace PlottingTool.Services;

public class ToolListener(
    EventBus eb,
    InputService inputService,
    CursorCoordinatesPicker cursorCoords,
    PlotterService plotterService
) : ILoadableSingleton, IInputProcessor
{
    const string ShowGuidelinesHotkey = "ShowGuidelines";
    const string LockHorizontalHotkey = "LockPlottingHorizontal";
    const string ClearAllPlotsHotkey = "ClearAllPlots";
    bool isInCursorTool;
    int? lockingHeight;

    public void Load()
    {
        eb.Register(this);
        inputService.AddInputProcessor(this);
    }

    [OnEvent]
    public void OnToolEntered(ToolEnteredEvent e)
    {
        if (e.Tool is not CursorTool) { return; }

        isInCursorTool = true;
        inputService.RemoveInputProcessor(this);
        inputService.AddInputProcessor(this);
    }

    [OnEvent]
    public void OnToolExit(ToolExitedEvent _)
    {
        isInCursorTool = false;
    }

    public bool ProcessInput()
    {
        var shouldReturnTrue = false;

        if (inputService.IsKeyDown(ClearAllPlotsHotkey))
        {
            plotterService.ClearAllBeacons();
            shouldReturnTrue = true;
        }

        if (inputService.IsKeyHeld(ShowGuidelinesHotkey))
        {
            var coord = cursorCoords.PickCoordinates(false);
            if (coord is null) { return false; }

            if (inputService.IsKeyDown(LockHorizontalHotkey))
            {
                lockingHeight = lockingHeight is null ? coord.Value.TileCoordinates.z : null;
                shouldReturnTrue = true;
            }

            if (isInCursorTool && !inputService.MouseOverUI && inputService.MainMouseButtonUp)
            {
                plotterService.ToggleBeacon(coord.Value.TileCoordinates.XY());
                return true;
            }

            plotterService.ShowHorizontalLines(lockingHeight ?? coord.Value.TileCoordinates.z);
        }
        else
        {
            if (inputService.IsKeyUp(ShowGuidelinesHotkey))
            {
                plotterService.DisableHorizontalLines();
            }
        }

        return shouldReturnTrue;
    }

}
