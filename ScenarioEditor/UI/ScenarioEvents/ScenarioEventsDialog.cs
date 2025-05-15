namespace ScenarioEditor.UI.ScenarioEvents;

public class ScenarioEventsDialog : DialogBoxElement
{

    public ScenarioEventsDialog(ILoc t)
    {
        SetTitle(t.T("LV.ScE.ScenarioEvents"));
        AddCloseButton();
    }

}
