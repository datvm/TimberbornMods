namespace DistroStorage.Services;

[BindSingleton]
public class DistroRegistry(
    MSettings s,
    IBlockService blocks
) : ILoadableSingleton
{
    readonly HashSet<IDistroSender> senders = [];
    readonly HashSet<IDistroReceiver> receivers = [];
    Vector3Int connectionRange;

    public IReadOnlyCollection<IDistroSender> Senders => senders;
    public IReadOnlyCollection<IDistroReceiver> Receivers => receivers;

    public void Load()
    {
        var range = s.Range.Value;
        connectionRange = new(range, range, range);
    }

    public void Register(IDistroComponent component)
    {
        var sender = component as IDistroSender;
        var receiver = component as IDistroReceiver;

        if (sender is not null)
        {
            ValidateAndAdd(sender, senders);
            ConnectNewComponent(sender, receivers);
        }
        else if (receiver is not null)
        {
            ValidateAndAdd(receiver, receivers);
            ConnectNewComponent(receiver, senders);
        }
        else
        {
            throw new InvalidOperationException($"Component {component} is neither a sender nor a receiver, and cannot be registered in the DistroRegistry.");
        }
    }

    public void Unregister(IDistroComponent r)
    {
        foreach (var comp in r.Connections)
        {
            comp.DisconnectFrom(r);
        }
        r.ClearConnections();

        if (r is IDistroSender sender)
        {
            senders.Remove(sender);
        }
        else if (r is IDistroReceiver receiver)
        {
            receivers.Remove(receiver);
        }
    }

    void ConnectNewComponent<T, TOther>(T component, HashSet<TOther> registeredList)
        where T : IDistroComponent
        where TOther : IDistroComponent
    {
        foreach (var other in FindRegisteredComponentsInRange(component.Bounds, registeredList))
        {
            component.ConnectWith(other);
            other.ConnectWith(component);
        }
    }

    void ValidateAndAdd<T>(T item, HashSet<T> list) where T : IDistroComponent
    {
        if (!list.Add(item))
        {
            throw new InvalidOperationException($"{typeof(T).Name} {item} is already registered in the DistroRegistry.");
        }
    }

    HashSet<T> FindRegisteredComponentsInRange<T>(BoundsInt bounds, HashSet<T> registeredList) where T : IDistroComponent
    {
        var (x1, y1, z1) = bounds.min - connectionRange;
        var (x2, y2, z2) = bounds.max + connectionRange; // Unity doc: max is EXCLUSIVE, so use < instead of <=
        HashSet<T> result = [];

        List<T> tmp = [];
        HashSet<BlockObject> visitedObjs = [];
        for (int x = x1; x < x2; x++)
        {
            for (int y = y1; y < y2; y++)
            {
                for (int z = z1; z < z2; z++)
                {
                    // Do not use GetObjectsWithComponent<T> because there are buildings with multiple T
                    var objs = blocks.GetObjectsAt(new(x, y, z)); 

                    foreach (var obj in objs)
                    {
                        if (!visitedObjs.Add(obj)) { continue; }

                        obj.GetComponents(tmp);
                        if (tmp.Count == 0) { continue; }

                        foreach (var other in tmp)
                        {
                            if (!other.SystemDisabled && registeredList.Contains(other))
                            {
                                result.Add(other);
                            }
                        }

                        tmp.Clear();
                    }
                }
            }
        }

        return result;
    }

}
