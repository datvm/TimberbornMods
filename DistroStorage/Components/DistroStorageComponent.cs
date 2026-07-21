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

    public IDistroSender? Sender => GetSingleActiveSender();
    public IDistroReceiver? Receiver => GetHighestPriority();

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

    IDistroSender? GetSingleActiveSender()
    {
        IDistroSender? found = null;
        foreach (var s in senders)
        {
            if (s.Active)
            {
                found = found is null
                    ? s
                    : throw new InvalidOperationException($"Multiple active {nameof(IDistroSender)} components found on entity {this}");
            }
        }

        return found;
    }

    IDistroReceiver? GetHighestPriority()
    {
        IDistroReceiver? found = null;
        
        foreach (var item in receivers)
        {
            if (!item.Active) { continue; }

            if (found is null || item.Priority > found.Priority)
            {
                found = item;
                continue;
            }
        }

        return found;
    }

}
