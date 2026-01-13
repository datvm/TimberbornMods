
namespace ConstructionNotifier.Components;

public class ConstructionSiteNotifier(ConstructionSiteNotifierService service) : BaseComponent, IPersistentEntity, IFinishedStateListener
{
    static readonly ComponentKey SaveKey = new(nameof(ConstructionSiteNotifier));
    static readonly PropertyKey<bool> NotifyKey = new("NotifyOnCompletion");
    static readonly PropertyKey<bool> NonblockingKey = new("Nonblocking");

    public bool NotifyOnCompletion { get; set; }
    public bool NonBlocking { get; set; }

    public void Load(IEntityLoader entityLoader)
    {
        if (!entityLoader.TryGetComponent(SaveKey, out var s)) { return; }

        if (s.Has(NotifyKey))
        {
            NotifyOnCompletion = s.Get(NotifyKey);
        }

        if (s.Has(NonblockingKey))
        {
            NonBlocking = s.Get(NonblockingKey);
        }
    }

    public void OnEnterFinishedState()
    {
        if (NotifyOnCompletion)
        {
            service.Notify(this, NonBlocking);
        }
    }

    public void OnExitFinishedState() { }

    public void Save(IEntitySaver entitySaver)
    {
        if (!NotifyOnCompletion) { return; }

        var bo = GetComponent<BlockObject>();
        if (bo.IsFinished) { return; }

        var s = entitySaver.GetComponent(SaveKey);
        s.Set(NotifyKey, true);
        s.Set(NonblockingKey, NonBlocking);
    }
}
