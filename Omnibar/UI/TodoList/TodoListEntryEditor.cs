namespace Omnibar.UI.TodoList;

public class TodoListEntryEditor : VisualElement
{

    public TodoListEntry? Entry { get; private set; }

#nullable disable
    readonly TodoListManager man;
    readonly IContainer container;

    readonly Button btnResetTimer;
    readonly Label lblTimer;
    readonly TextField txtTitle, txtNote;
    readonly Toggle chkTimer, chkPin, chkComplete;
    Toggle chkShowBuildingDetails;

    VisualElement buildingPanel, buildingPanelContent;
#nullable enable

    public TodoListEntryEditor(ILoc t, TodoListManager man, VisualElementLoader loader, IContainer container)
    {
        this.man = man;
        this.container = container;


        var parent = this.AddScrollView().SetMargin(left: 20);

        var src = loader.LoadVisualElement("Common/SteamWorkshop/SteamWorkshopUploadPanel");

        parent.AddGameLabel("LV.OB.TodoTitle".T(t));
        txtTitle = src.Q<TextField>("Name").SetMarginBottom();
        parent.Add(txtTitle);
        txtTitle.RegisterValueChangedCallback(evt =>
        {
            SetTitle(evt.newValue);
        });

        parent.AddGameLabel("LV.OB.TodoNote".T(t));
        txtNote = src.Q<TextField>("Description").SetMarginBottom();
        parent.Add(txtNote);
        txtNote.RegisterValueChangedCallback(evt =>
        {
            SetNote(evt.newValue);
        });

        var timer = parent.AddRow().AlignItems();
        chkTimer = timer.AddToggle("LV.OB.TodoTimer".T(t), onValueChanged: OnTimerChecked).SetMarginRight();
        lblTimer = timer.AddGameLabel("0").SetMarginRight();
        btnResetTimer = timer
            .AddGameButton("LV.OB.TodoTimerReset".T(t), onClick: () => OnTimerChecked(true))
            .SetPadding(paddingX: 10);

        var pin = parent.AddRow().AlignItems();
        chkPin = pin.AddToggle("LV.OB.TodoListPinned".T(t), onValueChanged: SetPin).SetMarginRight();
        pin.AddGameLabel("LV.OB.TodoListPinnedNote".T(t));

        var completed = parent.AddRow().AlignItems();
        chkComplete = completed.AddToggle("LV.OB.TodoListCompleted".T(t), onValueChanged: SetCompleted).SetMarginRight();
        completed.AddGameLabel("LV.OB.TodoListCompletedNote".T(t));

        AddBuildingPanel(parent, t);
    }

    void AddBuildingPanel(VisualElement parent, ILoc t)
    {
        buildingPanel = parent.AddChild().SetMargin(top: 20);
        buildingPanel.AddGameLabel(t.T("LV.OB.TodoBuildings").Bold());
        chkShowBuildingDetails = buildingPanel.AddToggle(t.T("LV.OB.TodoShowBuildingDetails"), onValueChanged: SetShowBuildingDetails)
            .SetMarginBottom(10);

        buildingPanelContent = buildingPanel.AddChild();
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

    void SetShowBuildingDetails(bool show)
    {
        Entry!.ShowBuildingDetails = show;
    }

    public void SetEntry(TodoListEntry? entry)
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
        buildingPanelContent.Clear();

        if (Entry?.Buildings is not null && Entry.Buildings.Count > 0)
        {
            chkShowBuildingDetails.SetValueWithoutNotify(Entry.ShowBuildingDetails);

            foreach (var item in Entry!.Buildings)
            {
                var buildingEl = container.GetInstance<TodoListEntryBuildingEditor>();
                buildingEl.DeleteRequested += DeleteEntryBuilding;

                buildingPanelContent.Add(buildingEl.Init(item));
            }
            buildingPanel.SetDisplay(true);
        }
        else
        {
            buildingPanel.SetDisplay(false);
        }
    }

    void DeleteEntryBuilding(TodoListEntryBuilding entry)
    {
        if (Entry is null) { return; }

        Entry.Buildings.Remove(entry);
        SetBuildingUI();
    }

}
