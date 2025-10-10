namespace ScientificProjects.UI;

public class SPDevPanel : CollapsiblePanel
{

    public event Action? OnActionExecuted;

    public SPDevPanel(IEnumerable<ISPDevModule> modules, DevModeManager devModeManager)
    {
        SetTitle("Dev Commands");

        if (!devModeManager.Enabled)
        {
            this.SetDisplay(false);
            return;
        }

        foreach (var m in modules)
        {
            foreach (var entry in m.GetEntries())
            {
                Container.AddGameButton(text: entry.Name, onClick: () => OnEntryClicked(entry), stretched: true)
                    .SetPadding(0, 3);
            }
        }
    }

    void OnEntryClicked(SPDevEntry entry)
    {
        Debug.Log("Executing Dev Entry: " + entry.Name);
        entry.Action();
        OnActionExecuted?.Invoke();
    }

}
