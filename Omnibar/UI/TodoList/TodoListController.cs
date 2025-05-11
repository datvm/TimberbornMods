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
        todoListPanel.OnToDoListRequested += OpenDialog;
    }

    public void AddBuilding(string prefabName, string buildingName)
    {        
        OpenDialog(prefabName, buildingName);
    }

    void OpenDialog(int? id)
    {
        OpenDialog(id, null, null);
    }

    void OpenDialog(string? addBuilding, string? buildingTitle)
    {
        OpenDialog(null, addBuilding, buildingTitle);
    }

    private async void OpenDialog(int? id, string? addBuilding, string? buildingTitle)
    {
        var diag = container.GetInstance<TodoListDialog>()
            .SetDialogSize(panelStack._root.panel.scaledPixelsPerPoint);

        if (id.HasValue)
        {
            diag.SetInitialItem(id.Value);
        }

        if (addBuilding is not null)
        {
            diag.AddNewItem(addBuilding, buildingTitle);
        }

        await diag.ShowAsync(null, panelStack);
        todoListPanel.ReloadList(null);
    }
}
