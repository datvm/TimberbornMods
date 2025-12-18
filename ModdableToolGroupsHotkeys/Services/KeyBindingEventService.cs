namespace ModdableToolGroupsHotkeys.Services;

public class KeyBindingEventService(InputService inputService) : IInputProcessor, ILoadableSingleton
{
    readonly Dictionary<string, KeyBindingEvent> mapper = [];

    public KeyBindingEvent Get(string id) => mapper.GetOrAdd(id, () => new(id));

    public void Load()
    {
        inputService.AddInputProcessor(this);
    }

    public bool ProcessInput()
    {
        foreach (var ev in mapper.Values)
        {
            if (inputService.IsKeyDown(ev.KeyBindingId))
            {
                ev.RaiseOnDown();
                return true;
            }
        }

        return false;
    }
}

public class KeyBindingEvent(string id)
{

    public readonly string KeyBindingId = id;

    public event Action? OnDown;
    internal void RaiseOnDown() => OnDown?.Invoke();

}