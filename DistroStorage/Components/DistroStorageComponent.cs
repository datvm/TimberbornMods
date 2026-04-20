namespace DistroStorage.Components;

[AddTemplateModule2(typeof(IDistroSender))]
[AddTemplateModule2(typeof(IDistroReceiver), AlsoBindTransient = false)]
public class DistroStorageComponent : BaseComponent, IAwakableComponent
{

    ImmutableArray<IDistroSender> senders = [];
    ImmutableArray<IDistroReceiver> receivers = [];

    public IDistroSender? Sender => GetSingleActive(senders);
    public IDistroReceiver? Receiver => GetSingleActive(receivers);

    public void Awake()
    {
        senders = [.. GetComponentsAllocating<IDistroSender>()];
        receivers = [.. GetComponentsAllocating<IDistroReceiver>()];
    }

    T? GetSingleActive<T>(ImmutableArray<T> list) where T : IDistroComponent
    {
        T? found = default;

        foreach (var item in list)
        {
            if (item.Active)
            {
                found = found is null
                    ? item
                    : throw new InvalidOperationException($"Multiple active {typeof(T).Name} components found on entity {Name}");
            }
        }

        return found;
    }

}
