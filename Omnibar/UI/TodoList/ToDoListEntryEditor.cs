namespace Omnibar.UI.TodoList;

public class ToDoListEntryEditor : VisualElement
{

    public ToDoListEntry? Entry { get; private set; }

#nullable disable
    readonly ToDoListManager man;
    readonly ILoc t;
    readonly IAssetLoader assetLoader;
    readonly ScienceService scienceService;
    readonly GoodItemFactory goodItemFactory;

    readonly Button btnResetTimer;
    readonly Label lblTimer;
    readonly TextField txtTitle, txtNote;
    readonly Toggle chkTimer, chkPin, chkComplete;

    VisualElement buildingPanel;
    Image icoBuilding;
    Button btnBuildingSub;
    Label lblBuildingName, lblBuildingQuan;
    VisualElement buildingCostPanel;

#nullable enable

    public ToDoListEntryEditor(ILoc t, ToDoListManager man, VisualElementLoader loader, IAssetLoader assetLoader, ScienceService scienceService, GoodItemFactory goodItemFactory)
    {
        this.t = t;
        this.man = man;
        this.assetLoader = assetLoader;
        this.scienceService = scienceService;
        this.goodItemFactory = goodItemFactory;

        var container = this.AddChild().SetMargin(left: 20);

        var src = loader.LoadVisualElement("Common/SteamWorkshop/SteamWorkshopUploadPanel");

        container.AddGameLabel("LV.OB.TodoTitle".T(t));
        txtTitle = src.Q<TextField>("Name").SetMarginBottom();
        container.Add(txtTitle);
        txtTitle.RegisterValueChangedCallback(evt =>
        {
            SetTitle(evt.newValue);
        });

        container.AddGameLabel("LV.OB.TodoNote".T(t));
        txtNote = src.Q<TextField>("Description").SetMarginBottom();
        container.Add(txtNote);
        txtNote.RegisterValueChangedCallback(evt =>
        {
            SetNote(evt.newValue);
        });

        var timer = container.AddRow().AlignItems();
        chkTimer = timer.AddToggle("LV.OB.TodoTimer".T(t), onValueChanged: OnTimerChecked).SetMarginRight();
        lblTimer = timer.AddGameLabel("0").SetMarginRight();
        btnResetTimer = timer
            .AddGameButton("LV.OB.TodoTimerReset".T(t), onClick: () => OnTimerChecked(true))
            .SetPadding(paddingX: 10);

        var pin = container.AddRow().AlignItems();
        chkPin = pin.AddToggle("LV.OB.TodoListPinned".T(t), onValueChanged: SetPin).SetMarginRight();
        pin.AddGameLabel("LV.OB.TodoListPinnedNote".T(t));

        var completed = container.AddRow().AlignItems();
        chkComplete = completed.AddToggle("LV.OB.TodoListCompleted".T(t), onValueChanged: SetCompleted).SetMarginRight();
        completed.AddGameLabel("LV.OB.TodoListCompletedNote".T(t));

        AddBuildingPanel(container);
    }

    void AddBuildingPanel(VisualElement parent)
    {
        buildingPanel = parent.AddChild().SetMargin(top: 20);

        var header = buildingPanel.AddRow().AlignItems();
        icoBuilding = header.AddImage()
            .SetSize(32, 32)
            .SetMarginRight();
        lblBuildingName = header.AddGameLabel();
        lblBuildingName.style.fontSize = 24;

        var quantity = buildingPanel.AddRow().AlignItems();
        quantity.AddGameLabel("LV.OB.TodoBuildingQuantity".T(t)).SetMarginRight();
        lblBuildingQuan = quantity.AddGameLabel("0").SetMarginRight();
        quantity.AddPlusButton().AddAction(() => SetQuantity(1));
        btnBuildingSub = quantity.AddMinusButton().AddAction(() => SetQuantity(-1));

        buildingCostPanel = buildingPanel.AddRow().AlignItems();
    }

    void SetQuantity(int delta)
    {
        if (Entry?.Building is null) { return; }

        Entry.BuildingQuantity += delta;
        SetBuildingQuantityUI();
    }

    void SetTitle(string title)
    {
        Entry!.Title = title;
        man.OnEntryChanged(Entry);
    }

    void SetNote(string note)
    {
        Entry!.Description = note;
    }

    void SetPin(bool pin)
    {
        Entry!.Pin = pin;
    }

    void SetCompleted(bool completed)
    {
        Entry!.Completed = completed;
        if (completed)
        {
            Entry.Pin = false;

        }

        man.Sort();
    }

    void OnTimerChecked(bool isChecked)
    {
        Entry!.Timer = isChecked ? 0 : null;
        SetTimerUi();
    }

    public void SetEntry(ToDoListEntry? entry)
    {
        Entry = entry;
        this.SetDisplay(entry is not null);
        if (entry is null) { return; }

        txtTitle.SetValueWithoutNotify(entry.Title);
        txtNote.SetValueWithoutNotify(entry.Description);
        chkComplete.SetValueWithoutNotify(entry.Completed);
        chkPin.SetValueWithoutNotify(entry.Pin);

        SetTimerUi();
        SetBuildingUI();
    }

    void SetTimerUi()
    {
        var hasTimer = Entry!.Timer is not null;

        chkTimer.SetValueWithoutNotify(hasTimer);
        lblTimer.SetDisplay(hasTimer);
        btnResetTimer.SetDisplay(hasTimer);

        if (hasTimer)
        {
            lblTimer.text = Entry.Timer!.Value.ToString("0.00");
        }
    }

    void SetBuildingUI()
    {
        if (Entry?.BuildingTool is null)
        {
            buildingPanel.SetDisplay(false);
            return;
        }

        var labelEntity = Entry.BuildingTool.Prefab.GetComponentFast<LabeledEntitySpec>();
        if (labelEntity.ImagePath is not null)
        {
            var icon = assetLoader.Load<Sprite>(labelEntity.ImagePath);
            icoBuilding.sprite = icon;
        }
        lblBuildingName.text = labelEntity.DisplayNameLocKey.T(t);

        SetBuildingQuantityUI();
        buildingPanel.SetDisplay(true);
    }

    void SetBuildingQuantityUI()
    {
        var quantity = Entry!.BuildingQuantity;
        lblBuildingQuan.text = quantity.ToString();
        btnBuildingSub.enabledSelf = Entry.BuildingQuantity > 1;

        var building = Entry.BuildingTool!.Prefab.GetComponentFast<BuildingSpec>();

        buildingCostPanel.Clear();

        var tool = Entry.BuildingTool!;
        if (tool.Locker is not null && building.ScienceCost > 0)
        {
            buildingCostPanel.AddChild(classes: ["science-cost-section__lock-icon"]);
            buildingCostPanel.AddGameLabel($"{scienceService.SciencePoints:#,0}/{building.ScienceCost:#,0}");
            buildingCostPanel.AddChild(classes: ["science-cost-section__science-icon"]).SetMarginRight();
        }

        foreach (var cost in building.BuildingCost)
        {
            buildingCostPanel.Add(goodItemFactory.Create(cost with { _amount = cost._amount * quantity }));
        }
    }

}
