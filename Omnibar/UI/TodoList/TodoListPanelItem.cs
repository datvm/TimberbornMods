namespace Omnibar.UI.TodoList;

public class TodoListPanelItem : VisualElement
{

    public ToDoListEntry Entry { get; }
    readonly Label? lblTimer;

    public TodoListPanelItem(ToDoListEntry entry, Texture2D timerIcon)
    {
        Entry = entry;

        this.SetMarginBottom();

        var lblTitle = this.AddGameLabel();
        lblTitle.text = entry.Title.Strikethrough(entry.Completed);

        if (entry.Timer is not null)
        {
            var timer = this.AddRow().AlignItems();

            var img = timer.AddImage()
                .SetSize(15, 15)
                .SetMarginRight(10);
            img.image = timerIcon;

            lblTimer = timer.AddGameLabel(entry.Timer.Value.ToString("0.00"));
        }
    }

    public void UpdateTimer()
    {
        if (lblTimer is not null && Entry.Timer is not null)
        {
            lblTimer.text = Entry.Timer.Value.ToString("0.00");
        }
    }

}
