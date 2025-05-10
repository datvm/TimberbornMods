namespace Omnibar.UI.TodoList;

public class ToDoListEntryEditor : VisualElement
{

    public ToDoListEntry? Entry { get; private set; }

#nullable disable
    ToDoListManager man;

    Button btnResetTimer;
    Label lblTimer;
    TextField txtTitle, txtNote;
    Toggle chkTimer, chkPin, chkComplete;
#nullable enable

    public ToDoListEntryEditor Init(ILoc t, ToDoListManager man, VisualElementLoader loader)
    {
        this.man = man;

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

        return this;
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

}
