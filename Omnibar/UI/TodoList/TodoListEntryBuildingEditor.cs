namespace Omnibar.UI.TodoList;

public class TodoListEntryBuildingEditor(
    ScienceService scienceService,
    GoodItemFactory goodItemFactory,
    IAssetLoader assetLoader,
    ILoc t
) : VisualElement
{

#nullable disable
    Image icoBuilding;
    Button btnBuildingSub;
    Label lblBuildingName, lblBuildingQuan;
    BuildingCostBox buildingCostBox;

    TodoListEntryBuilding buildingEntry;
#nullable enable

    public event Action<TodoListEntryBuilding>? DeleteRequested;

    public TodoListEntryBuildingEditor Init(TodoListEntryBuilding buildingEntry)
    {
        this.buildingEntry = buildingEntry;

        this.SetMarginBottom();

        AddElements(this);
        SetElements();

        return this;
    }

    void AddElements(VisualElement parent)
    {
        var header = parent.AddRow().AlignItems().SetMarginBottom();
        icoBuilding = header.AddImage()
            .SetSize(40)
            .SetMarginRight();
        lblBuildingName = header.AddGameLabel();
        lblBuildingName.style.fontSize = 24;


        var quantity = parent.AddRow().AlignItems();

        quantity.AddGameLabel("LV.OB.TodoBuildingQuantity".T(t)).SetMarginRight();
        lblBuildingQuan = quantity.AddGameLabel("0").SetMarginRight();
        
        quantity.AddPlusButton().AddAction(() => SetQuantity(1));
        btnBuildingSub = quantity.AddMinusButton().AddAction(() => SetQuantity(-1)).SetMarginRight();

        quantity.AddGameButton(t.T("LV.OB.Delete"), onClick: Remove).SetPadding(paddingX: 5);


        buildingCostBox = parent.AddChild<BuildingCostBox>();
    }

    void SetElements()
    {
        var labelEntity = buildingEntry.BuildingTool!.Prefab.GetComponentFast<LabeledEntitySpec>();
        if (labelEntity.ImagePath is not null)
        {
            var icon = assetLoader.Load<Sprite>(labelEntity.ImagePath);
            icoBuilding.sprite = icon;
        }
        lblBuildingName.text = labelEntity.DisplayNameLocKey.T(t);

        SetBuildingQuantityUI();
    }

    void SetQuantity(int delta)
    {
        buildingEntry.SetQuantity(buildingEntry.Quantity + delta);
        SetBuildingQuantityUI();
    }

    void SetBuildingQuantityUI()
    {
        var quantity = buildingEntry.Quantity;
        lblBuildingQuan.text = quantity.ToString();
        btnBuildingSub.enabledSelf = quantity > 1;

        var tool = buildingEntry.BuildingTool!;
        var buildingSpec = tool.Prefab.GetComponentFast<BuildingSpec>();

        buildingCostBox.SetMaterials(buildingSpec.BuildingCost.Multiply(quantity), goodItemFactory);
        if (tool.IsLocked())
        {
            buildingCostBox.SetScience(scienceService.SciencePoints, buildingSpec.ScienceCost);
        }
    }

    void Remove() => DeleteRequested?.Invoke(buildingEntry);

}
