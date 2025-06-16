namespace Omnibar.Services.TodoList;

public class TodoListUpdater(
    TodoListManager man
) : ITickableSingleton
{

    public void Tick()
    {
        var timeChanged = Time.fixedDeltaTime;

        foreach (var item in man.Entries)
        {
            if (item.Timer.HasValue)
            {
                item.Timer += timeChanged;
            }
        }
    }
}
