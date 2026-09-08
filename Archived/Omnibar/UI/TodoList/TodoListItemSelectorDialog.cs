namespace Omnibar.UI.TodoList;

public class TodoListItemSelectorDialog : DialogBoxElement
{
    readonly Dropdown cboItems;
    readonly List<TodoListEntry> entries;

    public TodoListEntry SelectedEntry => entries[cboItems.GetSelectedIndex()];

    public TodoListItemSelectorDialog(VisualElementInitializer veInit, TodoListManager man, DropdownItemsSetter setter, ILoc t)
    {
        SetTitle(t.T("LV.OB.TodoList"));
        AddCloseButton();

        entries = man.Entries;
        cboItems = Content.AddDropdown().SetMarginBottom();

        var buttons = Content.AddRow().JustifyContent();
        buttons.AddMenuButton(t.T("Core.OK"), onClick: OnUIConfirmed);
        buttons.AddMenuButton(t.T("Core.Cancel"), onClick: OnUICancelled);
        
        this.Initialize(veInit);

        cboItems.SetItems(
            setter,
            [.. entries.Select(q => q.Title)]);
    }

}
