namespace ScientificProjects.UI;

partial class ScientificProjectDialog
{

    public void AddDevMode()
    {
        var row = this.AddChild().SetMarginBottom();

        row.AddGameLabel("Dev buttons: ");
        AddDevButton(row, "Clear all unlocks", DevClearAllUpgrades);
        AddDevButton(row, "Set science to 0", DevSetScienceTo0);
        AddDevButton(row, "Set science to Daily Cost", DevSetToDailyCost);
        AddDevButton(row, "Add 100 science", DevAdd100Science);
        AddDevButton(row, "Remove 100 science", DevRemove100Science);
        AddDevButton(row, "Print dialog UXML & USS to Log", PrintUi);

        row.InsertSelfBefore(Content.Children().First());
    }

    void AddDevButton(VisualElement el, string text, Action action)
    {
        el.AddGameButton(text: text, onClick: action)
            .SetFlexGrow()
            .SetMarginBottom(5);
    }

    void DevClearAllUpgrades()
    {
        projects.ClearAllUpgrades();
        RefreshContent();
    }

    void PrintUi()
    {
        this.PrintVisualTree(true);
        this.PrintStylesheet();
    }

    void DevSetScienceTo0() => DevSetScienceTo(0);
    void DevAdd100Science() => DevSetScienceTo(sciences.SciencePoints + 100);
    void DevRemove100Science() => DevSetScienceTo(Math.Max(0, sciences.SciencePoints - 100));
    void DevSetToDailyCost() => DevSetScienceTo(dailyCost);

    void DevSetScienceTo(int science)
    {
        sciences.AddPoints(-sciences.SciencePoints + science);
        RefreshContent();
    }

}
