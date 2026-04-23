namespace DistroStorage.Components;

[AddTemplateModule2(typeof(IDistroSender))]
[AddTemplateModule2(typeof(IDistroReceiver), AlsoBindTransient = false)]
public class DistroStorageComponent : BaseComponent, IAwakableComponent
{

#nullable disable
    BlockObject blockObject;
    BlockObjectCenter blockObjectCenter;
#nullable enable

    ImmutableArray<IDistroSender> senders = [];
    ImmutableArray<IDistroReceiver> receivers = [];

    public IDistroSender? Sender => GetSingleActive(senders);
    public IDistroReceiver? Receiver => GetSingleActive(receivers);

    public Vector3 AboveCenter
    {
        get
        {
            var y = blockObject.Coordinates.z + blockObject._blockObjectSpec.Size.z + .5f;
            return blockObjectCenter.WorldCenter with { y = y };
        }
    }

    public void Awake()
    {
        blockObjectCenter = GetComponent<BlockObjectCenter>();
        blockObject = GetComponent<BlockObject>();

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
