namespace Omnibar.Services.Omnibar.Providers;

public class OmnibarTodoListProvider(
    ILoc t,
    IAssetLoader assets,
    TodoListController todoListController
) : IOmnibarCommandProvider, ILoadableSingleton
{
    public const string TimerSwitch = "+timer";
    public const string TodoCommand = "/todo ";
    public const string TodoWithTimerCommand = TodoCommand + TimerSwitch;

    public string Command { get; } = TodoCommand;
    public string CommandDesc { get; } = "LV.OB.CommandDescTodo".T(t, TodoCommand, TimerSwitch);
    public Texture2D Icon { get; private set; } = null!;

    OmnibarFilteredItem[] timerHint = null!;
    OmnibarFilteredItem[] executeItem = null!;

    public void Load()
    {
        Icon = assets.Load<Texture2D>("Sprites/Omnibar/todo");
        timerHint = [new(new OmnibarTodoListTimerItem(t, Icon, CommandDesc), FuzzyMatchResult.Empty)];
        executeItem = [new(new OmnibarTodoListAddItem(t, Icon, CommandDesc, todoListController), FuzzyMatchResult.Empty)];
    }

    public IReadOnlyList<OmnibarFilteredItem> ProvideItems(string filter)
    {
        if (!filter.StartsWith(Command, StringComparison.OrdinalIgnoreCase)) { return []; }

        if (TodoWithTimerCommand.StartsWith(filter, StringComparison.OrdinalIgnoreCase))
        {
            return timerHint;
        }

        return executeItem;
    }

}

public class OmnibarTodoListTimerItem(ILoc t, Texture2D icon, string desc) : IInplaceExecutionOmnibarItem
{
    public string Title { get; } = "LV.OB.CommandAddTodo".T(t);
    public IOmnibarDescriptor? Description { get; } = new SimpleLabelDescriptor(desc);

    public void Execute() => throw new NotImplementedException();

    public void Execute(OmnibarBox box)
    {
        var command = box.Text;

        box.SetText(command.Length < OmnibarTodoListProvider.TodoCommand.Length
            ? OmnibarTodoListProvider.TodoCommand
            : (OmnibarTodoListProvider.TodoWithTimerCommand + " ")
        );
    }

    public bool SetIcon(Image image)
    {
        image.image = icon;
        return true;
    }
}

public class OmnibarTodoListAddItem(ILoc t, Texture2D icon, string desc, TodoListController todoListController) : IInplaceExecutionOmnibarItem
{
    public string Title { get; } = "LV.OB.CommandAddTodo".T(t);
    public IOmnibarDescriptor? Description { get; } = new SimpleLabelDescriptor(desc);

    public void Execute() => throw new NotImplementedException();

    public void Execute(OmnibarBox box)
    {
        var command = box.Text;
        if (command.Length < OmnibarTodoListProvider.TodoCommand.Length) { return; } // Should not happen

        var hasTimer = command.StartsWith(OmnibarTodoListProvider.TodoWithTimerCommand, StringComparison.OrdinalIgnoreCase);

        var todoText = command.Substring(hasTimer
            ? OmnibarTodoListProvider.TodoWithTimerCommand.Length + 1 :
            OmnibarTodoListProvider.TodoCommand.Length);

        todoListController.AddItem(todoText, hasTimer);
    }

    public bool SetIcon(Image image)
    {
        image.image = icon;
        return true;
    }
}