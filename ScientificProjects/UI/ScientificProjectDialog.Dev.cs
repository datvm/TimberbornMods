namespace ScientificProjects.UI;

partial class ScientificProjectDialog
{

    public void AddDevMode()
    {
        var row = this.AddChild().SetAsRow().SetMarginBottom();

        row.AddGameLabel("Dev buttons: ");
        row.AddGameButton("Clear all unlocks", DevClearAllUpgrades);

        row.InsertSelfBefore(Content.Children().First());
    }

    void DevClearAllUpgrades()
    {
        projects.ClearAllUpgrades();
        RefreshContent();
    }

}
