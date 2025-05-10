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
        todoListPanel.OnToDoListRequested += OpenDialog;
    }

    private async void OpenDialog(int? id)
    {
        var diag = container.GetInstance<TodoListDialog>()
            .SetDialogSize(panelStack._root.panel.scaledPixelsPerPoint);
        if (id.HasValue)
        {
            diag.SetInitialItem(id.Value);
        }

        await diag.ShowAsync(null, panelStack);
        todoListPanel.ReloadList(null);
    }
}
