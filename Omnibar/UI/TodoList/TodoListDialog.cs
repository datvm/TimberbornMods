namespace Omnibar.UI.TodoList;

public class TodoListDialog : DialogBoxElement
{

    ToDoListDialogListView lstItems = null!;
    ToDoListEntryEditor editor = null!;

    readonly ToDoListManager man;
    readonly ILoc t;
    readonly DialogBoxShower diagShower;
    readonly VisualElementLoader veLoader;

    readonly VisualElement contentWrapper;

    public TodoListDialog(
        ILoc t,
        VisualElementInitializer veInit,
        ToDoListManager man,
        DialogBoxShower diagShower,
        VisualElementLoader veLoader
    )
    {
        this.t = t;
        this.man = man;
        this.diagShower = diagShower;
        this.veLoader = veLoader;

        SetTitle("LV.OB.TodoList".T(t));
        AddCloseButton();

        contentWrapper = Content.AddChild();
        AddHeader(contentWrapper);
        AddSplitView(contentWrapper);

        this.Initialize(veInit);
        lstItems.ReloadTodoList();
    }

    public TodoListDialog SetDialogSize(float scale)
    {
        var w = .8f * Screen.width / scale;
        var h = .8f * Screen.height / scale;

        this.Q("Box").SetWidth(w);
        contentWrapper.SetHeight(h);

        this.SetMinMaxSize(w, null);

        return this;
    }

    void AddHeader(VisualElement parent)
    {
        var header = parent.AddRow().SetFlexShrink(0).SetMarginBottom();

        header.AddMenuButton("LV.OB.TodoListAdd".T(t), AddNewItem);
        header.AddMenuButton("LV.OB.ClearCompleted".T(t), ClearCompleted);
        header.AddMenuButton("LV.OB.TodoListRemove".T(t), AttemptDelete);
    }

    VisualElement AddSplitView(VisualElement parent)
    {
        var row = parent.AddRow().SetFlexGrow();

        lstItems = row.AddChild<ToDoListDialogListView>()
            .Init(man, veLoader)
            .SetFlexGrow(0)
            .SetFlexShrink(0);
        lstItems.SetWidthPercent(30);

        lstItems.OnEntrySelected += OnEntrySelected;

        editor = row.AddChild<ToDoListEntryEditor>()
            .SetFlexGrow(1)
            .Init(t, man, veLoader);

        return row;
    }

    private void OnEntrySelected(ToDoListEntry? obj)
    {
        editor.SetEntry(obj);
    }

    void AddNewItem()
    {
        var entry = man.Add(new()
        {
            Title = "LV.OB.NewEntryTitle".T(t),
            Pin = true,
        });

        lstItems.SelectItem(entry.Id);
    }

    void ClearCompleted()
    {
        diagShower.Create()
            .SetMessage("LV.OB.ClearCompletedConfirm".T(t))
            .SetConfirmButton(man.ClearCompleted)
            .SetDefaultCancelButton()
            .Show();
    }

    void AttemptDelete()
    {
        var item = lstItems.SelectedEntry;
        if (item is null)
        {
            diagShower.Create()
                .SetMessage("LV.OB.NoItemSelected".T(t))
                .Show();
            return;
        }

        diagShower.Create()
            .SetMessage("LV.OB.RemoveConfirm".T(t))
            .SetConfirmButton(() => man.Remove(item))
            .SetDefaultCancelButton()
            .Show();
    }

    public void SetInitialItem(int id)
    {
        lstItems.SelectItem(id);
    }

}
