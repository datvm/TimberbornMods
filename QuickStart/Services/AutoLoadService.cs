using UnityEngine.InputSystem;

namespace QuickStart.Services;

public class AutoLoadService(
    PanelStack panelStack,
    MainMenuPanel mainMenuPanel,
    QuickMapEditorService quickMapEditorService,
    ILoc t
) : IPostLoadableSingleton, IUpdatableSingleton
{
    const float DelayTime = 5f;

    bool enabled;
    bool shiftPressed;
    float? startTime;

    static bool firstTime = true;
    Label? lblPrompt;

    public void PostLoad()
    {
        if (!MSettings.AutoContinueShift || !firstTime) { return; }
        firstTime = false;

        AppendLabel();
        enabled = true;
    }

    public void UpdateSingleton()
    {
        if (!enabled || shiftPressed) { return; }

        if (Keyboard.current.shiftKey.isPressed)
        {
            shiftPressed = true;
            PerformAction();
            return;
        }

        if (startTime is null)
        {
            var panel = panelStack._stack.FirstOrDefault();
            if (panel.PanelController is MainMenuPanel)
            {
                startTime = Time.unscaledTime;
                Debug.Log("Start time: " + startTime);
            }
        }
        else if (Time.unscaledTime - startTime >= DelayTime)
        {
            Debug.Log("Activation time: " + Time.unscaledTime);

            enabled = false;
            PerformAction();
        }
    }

    void AppendLabel()
    {
        var label = lblPrompt = new Label();
        label.style.backgroundColor = Color.black;
        label.style.color = Color.white;
        label.style.fontSize = 16;
        label.style.unityFontStyleAndWeight = FontStyle.Bold;
        label.style.position = Position.Absolute;
        label.style.left = 10;
        label.style.top = 10;

        string loc;
        if (MSettings.AutoLoadMap)
        {
            loc = MSettings.AutoContinueShiftToCancel ? "LV.QS.PromptCancelLoadMap" : "LV.QS.PromptLoadMap";
        }
        else
        {
            loc = MSettings.AutoContinueShiftToCancel ? "LV.QS.PromptCancelContinue" : "LV.QS.PromptContinue";
        }
        label.text = t.T(loc);

        mainMenuPanel._root.Add(label);
    }

    void PerformAction()
    {
        if (lblPrompt is not null)
        {
            lblPrompt.RemoveFromHierarchy();
            lblPrompt = null;
        }

        if (MSettings.AutoContinueShiftToCancel == shiftPressed) { return; }

        if (MSettings.AutoLoadMap)
        {
            quickMapEditorService.LoadLatestMap();
        }
        else
        {
            mainMenuPanel.ContinueClicked(null);
        }
    }

}
