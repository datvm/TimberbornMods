using ZiplineTool = Timberborn.ZiplineSystemUI.ZiplineConnectionAddingTool;

namespace Ziporter.Services;

public class ZiporterConnectionTool(
    InputService inputService,
    CursorService cursorService,
    EntitySelectionService entitySelectionService,
    SelectableObjectRaycaster selectableObjectRaycaster,
    ZiporterConnectionService ziporterConnectionService,
    UISoundController uiSoundController,
    Highlighter highlighter,
    ToolManager toolManager
) : Tool, IInputProcessor
{

    public ZiporterConnection? From { get; set; }

    public override void Enter()
    {
        Asserts.FieldIsNotNull(this, From, nameof(From));
        inputService.AddInputProcessor(this);
        cursorService.SetCursor(ZiplineTool.CursorKey);
    }

    public override void Exit()
    {
        inputService.RemoveInputProcessor(this);
        cursorService.ResetCursor();
        entitySelectionService.Select(From);
        highlighter.UnhighlightAllSecondary();
        From = null;
    }

    public bool ProcessInput()
    {
        var ziporter = selectableObjectRaycaster.TryHitSelectableObject(out var hitObject)
            ? hitObject.GetComponentFast<ZiporterConnection>()
            : null;

        highlighter.UnhighlightAllSecondary();
        var clickingObj = inputService.MainMouseButtonDown && !inputService.MouseOverUI;

        if (ziporter && ziporterConnectionService.CanBeConnected(From, ziporter))
        {
            if (clickingObj)
            {
                Connect(ziporter);
                uiSoundController.PlayClickSound();
                return true;
            }
            else
            {
                highlighter.HighlightSecondary(ziporter, Color.green);
            }
        }
        else if (clickingObj)
        {
            uiSoundController.PlayCantDoSound();
        }

        return false;
    }

    void Connect(ZiporterConnection to)
    {
        ziporterConnectionService.Connect(From!, to);
        toolManager.SwitchToDefaultTool();
        entitySelectionService.Select(From);
    }
}
