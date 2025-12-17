namespace ModdableToolGroupsHotkeys.Services;

public class KeyBindingEventService : IUnloadableSingleton
{
    public static KeyBindingEventService? Instance { get; private set; }

    readonly Dictionary<KeyBinding, KeyBindingEvent> mapper = [];

    public KeyBindingEvent Get(KeyBinding keyBinding) => mapper.GetOrAdd(keyBinding);

    public KeyBindingEventService()
    {
        Instance = this;
    }

    public void Unload()
    {
        Instance = null;
    }

    internal void RaiseOnDown(KeyBinding keyBinding)
    {
        if (mapper.TryGetValue(keyBinding, out var ev))
        {
            ev.RaiseOnDown();
        }
    }

}

public class KeyBindingEvent
{

    public event Action? OnDown;
    internal void RaiseOnDown() => OnDown?.Invoke();

}