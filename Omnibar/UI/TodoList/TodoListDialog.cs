﻿using UnityEngine.Assertions.Must;

namespace Omnibar.UI.TodoList;

public class TodoListDialog : DialogBoxElement
{

    TodoListDialogListView lstItems = null!;
    TodoListEntryEditor editor = null!;

    readonly TodoListManager man;
    readonly ILoc t;
    readonly DialogBoxShower diagShower;
    readonly VisualElementLoader veLoader;
    readonly IContainer container;

    readonly VisualElement contentWrapper;

    public TodoListDialog(
        ILoc t,
        VisualElementInitializer veInit,
        TodoListManager man,
        DialogBoxShower diagShower,
        VisualElementLoader veLoader,
        IContainer container
    )
    {
        this.t = t;
        this.man = man;
        this.diagShower = diagShower;
        this.veLoader = veLoader;
        this.container = container;

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

        lstItems = row.AddChild<TodoListDialogListView>()
            .Init(man, veLoader)
            .SetFlexGrow(0)
            .SetFlexShrink(0);
        lstItems.SetWidthPercent(30);

        lstItems.OnEntrySelected += OnEntrySelected;

        editor = container.GetInstance<TodoListEntryEditor>()
            .SetFlexGrow(1);
        row.Add(editor);

        return row;
    }

    private void OnEntrySelected(TodoListEntry? obj)
    {
        editor.SetEntry(obj);
    }

    public void AddNewItem() => AddNewItem(null);
    public void AddNewItem(string? building, string? title = null, bool timer = false)
    {
        var entry = man.Add(new()
        {
            Title = string.IsNullOrWhiteSpace(title) ? "LV.OB.NewEntryTitle".T(t) : title!,
            Pin = true,
            Buildings = building is null ? [] : [new(building, 1)],
            Timer = timer ? 0 : null,
        });

        lstItems.SelectItem(entry.Id);
    }

    public void AppendNewItem(TodoListEntry entry, string building)
    {
        entry.Buildings.Add(new(building, 1));
        man.OnEntryChanged(entry);

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
