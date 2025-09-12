namespace ModdableTimberborn.Common;

public interface ITogglableContainer<TContainer, TMember>
{
    event Action<bool>? Toggled;

    TContainer Container { get; }
    IEnumerable<TMember> Members { get; }
    bool Active { get; }
    
    public void Toggle(bool active);
    public void Activate() => Toggle(true);
    public void Deactivate() => Toggle(false);
}
