namespace ScenarioEditor.UI.ScenarioEvents;

public class ScenarioEventsController(
    FilePanel filePanel,
    ScenarioEventsDialog diag,
    PanelStack panelStack
) : ILoadableSingleton
{

    public void Load()
    {
        filePanel.AddButton(OpenEventDialog, "LV.ScE.ScenarioEvents");
    }

    async void OpenEventDialog()
    {
        await diag.ShowAsync(null, panelStack);
    }

}
