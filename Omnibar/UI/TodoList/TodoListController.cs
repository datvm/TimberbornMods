namespace Omnibar.UI.TodoList;

public class TodoListController(
    UILayout uiLayout,
    TodoListPanel todoListPanel,
    IContainer container,
    PanelStack panelStack
) : ILoadableSingleton
{

    public void Load()
    {
        uiLayout.AddBottomLeft(todoListPanel.Root, -200);
        todoListPanel.OnTodoListRequested += OpenDialog;
    }

    public async void AddBuildingAsync(string prefabName, string buildingName, bool append)
    {
        TodoListEntry? appendingEntry = null;
        if (append)
        {
            var diag = container.GetInstance<TodoListItemSelectorDialog>();

            var result = await diag.ShowAsync(null, panelStack);
            if (!result) { return; }

            appendingEntry = diag.SelectedEntry;
        }

        InternalOpenDialog(addBuilding: prefabName, title: buildingName, appendingEntry: appendingEntry);
    }

    public void AddItem(string title, bool timer)
    {
        InternalOpenDialog(title: title, timer: timer);
    }

    public void OpenDialog(int? id = default)
    {
        InternalOpenDialog(id: id);
    }

    private async void InternalOpenDialog(int? id = null, string? addBuilding = null, string? title = null, TodoListEntry? appendingEntry = null, bool timer = false)
    {
        var diag = container.GetInstance<TodoListDialog>()
            .SetDialogSize(panelStack._root.panel.scaledPixelsPerPoint);

        if (id.HasValue)
        {
            diag.SetInitialItem(id.Value);
        }

        if (appendingEntry is not null)
        {
            diag.AppendNewItem(appendingEntry, addBuilding ?? throw new ArgumentNullException(nameof(addBuilding)));
        }
        else if (addBuilding is not null || title is not null)
        {
            diag.AddNewItem(building: addBuilding, title: title, timer: timer);
        }

        await diag.ShowAsync(null, panelStack);
        todoListPanel.ReloadList(null);
    }
}
