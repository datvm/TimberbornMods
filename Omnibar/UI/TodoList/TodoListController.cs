namespace Omnibar.UI.TodoList;

public class TodoListController(
    UILayout uiLayout,
    TodoListPanel todoListPanel,
    IContainer container,
    PanelStack panelStack
) : ILoadableSingleton
{
    public static readonly KeyCode AddBuildingToTodoListKeyCode = KeyCode.F5;
    public static readonly string AddBuildingToTodoListKeyName = nameof(KeyCode.F5).Bold().Color(TimberbornTextColor.Solid);

    public void Load()
    {
        uiLayout.AddBottomLeft(todoListPanel.Root, -200);
        todoListPanel.OnToDoListRequested += OpenDialogWithId;
    }

    public void AddBuilding(string prefabName, string buildingName)
    {        
        OpenDialog(addBuilding: prefabName, title: buildingName);
    }

    public void AddItem(string title, bool timer)
    {
        OpenDialog(title: title, timer: timer);
    }

    void OpenDialogWithId(int? id)
    {
        OpenDialog(id: id);
    }

    private async void OpenDialog(int? id = null, string? addBuilding = null, string? title = null, bool timer = false)
    {
        var diag = container.GetInstance<TodoListDialog>()
            .SetDialogSize(panelStack._root.panel.scaledPixelsPerPoint);

        if (id.HasValue)
        {
            diag.SetInitialItem(id.Value);
        }

        if (addBuilding is not null || title is not null)
        {
            diag.AddNewItem(addBuilding, title, timer);
        }

        await diag.ShowAsync(null, panelStack);
        todoListPanel.ReloadList(null);
    }
}
