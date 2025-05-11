namespace Omnibar.UI.TodoList;

public class TodoListPanelItem : VisualElement
{

    public ToDoListEntry Entry { get; }
    readonly ScienceService scienceService;

    readonly Label? lblTimer;

    VisualElement? sciencePanel;
    Label? lblScience;

    public TodoListPanelItem(ToDoListEntry entry, Texture2D timerIcon, ScienceService scienceService, GoodItemFactory goodItemFactory)
    {
        Entry = entry;
        this.scienceService = scienceService;

        this.SetMarginBottom();

        var lblTitle = this.AddGameLabel();
        lblTitle.text = GetTitle(entry);

        if (entry.Timer is not null)
        {
            var timer = this.AddRow().AlignItems();

            var img = timer.AddImage()
                .SetSize(15, 15)
                .SetMarginRight(10);
            img.image = timerIcon;

            lblTimer = timer.AddGameLabel(entry.Timer.Value.ToString("0.00"));
        }

        if (entry.BuildingTool is not null)
        {
            var costs = this.AddRow().AlignItems();
            costs.style.flexWrap = Wrap.Wrap;

            var buildingSpec = entry.BuildingTool.Prefab.GetComponentFast<BuildingSpec>();

            if (entry.BuildingTool.Locker is not null)
            {
                sciencePanel = costs.AddRow().AlignItems().SetMarginRight(10);

                sciencePanel.AddChild(classes: ["science-cost-section__lock-icon"]);
                lblScience = sciencePanel.AddGameLabel("0");
                sciencePanel.AddGameLabel("/" + buildingSpec.ScienceCost.ToString("#,0"));
                sciencePanel.AddChild(classes: ["science-cost-section__science-icon"]);
            }

            var quantity = entry.BuildingQuantity;
            foreach (var cost in buildingSpec.BuildingCost)
            {
                costs.Add(goodItemFactory.Create(cost with { _amount = cost._amount * quantity }));
            }
        }
    }

    static string GetTitle(ToDoListEntry entry)
    {
        return (entry.Title +
            (entry.BuildingQuantity > 1 ? $" (x{entry.BuildingQuantity})" : ""))
            .Bold()
            .Strikethrough(entry.Completed);
    }

    public void UpdateData()
    {
        if (lblTimer is not null && Entry.Timer is not null)
        {
            lblTimer.text = Entry.Timer.Value.ToString("0.00");
        }

        if (lblScience is not null)
        {
            if (Entry.BuildingTool?.Locker is null)
            {
                sciencePanel!.RemoveSelf();
                sciencePanel = null;
                lblScience = null;
            }
            else
            {
                lblScience.text = scienceService.SciencePoints.ToString("#,0");
            }
        }
    }

}
