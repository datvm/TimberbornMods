namespace Omnibar.UI.TodoList;

public class TodoListPanelItem : VisualElement
{
    static readonly Color SeparatorColor = new(128 / 255f, 161 / 255f, 108 / 255f);

    public TodoListEntry Entry { get; }

    readonly Label? lblTimer;
    BuildingCostBox[]? buildingCostBoxes;
    readonly TodoListPanelItemBuildingTotal? totalEntry;
    readonly ScienceService scienceService;

    public TodoListPanelItem(TodoListEntry entry, Texture2D timerIcon, ScienceService scienceService, GoodItemFactory goodItemFactory, ILoc t)
    {
        Entry = entry;
        this.scienceService = scienceService;

        this.SetMargin(marginY: 10);

        style.borderTopWidth = 1;
        style.borderTopColor = SeparatorColor;

        var lblTitle = this.AddGameLabel();
        lblTitle.text = GetTitle(entry).Bold().Color(TimberbornTextColor.Solid);

        if (entry.Timer is not null)
        {
            var timer = this.AddRow().AlignItems();

            var img = timer.AddImage()
                .SetSize(15, 15)
                .SetMarginRight(10);
            img.image = timerIcon;

            lblTimer = timer.AddGameLabel(entry.Timer.Value.ToString("0.00"));
        }

        var buildingsCount = entry.Buildings.Count;
        if (buildingsCount > 0)
        {
            var buildingPanel = this.AddChild();

            var buildings = entry.Buildings.Select(q => new TodoListBuildingDetails(q)).ToArray();
            var showDetails = entry.ShowBuildingDetails;

            if (showDetails || buildingsCount == 1)
            {
                buildingCostBoxes = new BuildingCostBox[buildings.Length];
            }

            var science = scienceService.SciencePoints;
            for (int i = 0; i < buildingsCount; i++)
            {
                var building = buildings[i];

                buildingPanel.AddLabel($"{t.T(building.LabelSpec.DisplayNameLocKey)} x{building.Entry.Quantity}");

                if (showDetails || buildingsCount == 1)
                {
                    var box = buildingCostBoxes![i] = buildingPanel.AddChild<BuildingCostBox>();
                    box.Building = building;
                    box.SetMaterials(building.BuildingSpec.BuildingCost.Multiply(building.Entry.Quantity), goodItemFactory);

                    if (building.Entry.IsLocked())
                    {
                        box.SetScience(science, building.BuildingSpec.ScienceCost);
                    }
                }
            }

            if (buildingsCount > 1)
            {
                if (showDetails)
                {
                    this.AddGameLabel(t.T("LV.OB.Total").Color(TimberbornTextColor.Solid));
                }

                totalEntry = this
                    .AddChild<TodoListPanelItemBuildingTotal>()
                    .Init(buildings, goodItemFactory, scienceService);
            }
        }

    }

    static string GetTitle(TodoListEntry entry)
    {
        return entry.Title
            .Bold()
            .Strikethrough(entry.Completed);
    }

    public void UpdateData()
    {
        if (lblTimer is not null && Entry.Timer is not null)
        {
            lblTimer.text = Entry.Timer.Value.ToString("0.00");
        }

        totalEntry?.UpdateData();

        if (buildingCostBoxes is not null)
        {
            UpdateBuildingSciences();
        }
    }

    void UpdateBuildingSciences()
    {
        var hasScience = false;
        var science = scienceService.SciencePoints;

        foreach (var box in buildingCostBoxes!)
        {
            if (!box.HasScience) { continue; }

            var building = box.Building!.Value;
            var isLocked = building.Entry.IsLocked();

            if (isLocked)
            {
                box.SetScience(science, null);
                hasScience = true;
            }
            else
            {
                box.SetScience(null, null);
            }
        }

        if (!hasScience)
        {
            buildingCostBoxes = null;
        }
    }

}
