namespace ModdableTimberborn.Common;

public interface ITogglableContainer<TContainer, TMember>
{
    event Action<bool>? Toggled;

    TContainer Container { get; }
    IEnumerable<TMember> Members { get; }
    bool Active { get; }
    
    void Toggle(bool active);
    void Activate() => Toggle(true);
    void Deactivate() => Toggle(false);
}
