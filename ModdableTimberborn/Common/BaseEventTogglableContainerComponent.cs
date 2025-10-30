
namespace ModdableTimberborn.Common;

public abstract class BaseEventTogglableContainerComponent<TTogglable, TContainer, TMember> : BaseComponent, ITogglableContainer<TContainer, TMember>
    where TTogglable : BaseEventTogglableContainer<TContainer, TMember>
    where TContainer : BaseComponent
    where TMember : BaseComponent
{
    protected TTogglable data = null!;

    public TContainer Container => data.Container;
    public IEnumerable<TMember> Members => data.Members;
    public bool Active => data.Active;

    public event Action<bool>? Toggled;

    public virtual void Awake()
    {
        data = CreateData(GetComponent<TContainer>());
        data.Toggled += e => Toggled?.Invoke(e);
    }

    public void Toggle(bool active) => data.Toggle(active);

    protected abstract TTogglable CreateData(TContainer comp);
}
