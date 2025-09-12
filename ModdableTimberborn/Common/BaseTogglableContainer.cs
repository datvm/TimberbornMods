
namespace ModdableTimberborn.Common;

public abstract class BaseTogglableContainer<TContainer, TMember> : ITogglableContainer<TContainer, TMember>
{
    public abstract TContainer Container { get; }
    public abstract IEnumerable<TMember> Members { get; }
    public bool Active { get; private set; }

    public event Action<bool>? Toggled;

    public void Toggle(bool active)
    {
        if (active == Active) { return; }

        Active = active;
        if (active)
        {
            PerformActivate();
        }
        else
        {
            PerformDeactivate();
        }

        Toggled?.Invoke(active);
    }

    protected abstract void PerformActivate();
    protected abstract void PerformDeactivate();
}

public abstract class BaseEventTogglableContainer<TContainer, TMember> : BaseTogglableContainer<TContainer, TMember>
{
    
    protected override void PerformActivate()
    {
        AddEventHandlers();
        foreach (var member in Members)
        {
            OnMemberAdded(member);
        }
    }

    protected override void PerformDeactivate()
    {
        RemoveEventHandlers();
        foreach (var member in Members)
        {
            OnMemberRemoved(member);
        }
    }

    protected abstract void AddEventHandlers();
    protected abstract void RemoveEventHandlers();

    protected abstract void OnMemberAdded(TMember member);
    protected abstract void OnMemberRemoved(TMember member);
}