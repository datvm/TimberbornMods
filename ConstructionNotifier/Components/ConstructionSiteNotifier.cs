
namespace ConstructionNotifier.Components;

public class ConstructionSiteNotifier(ConstructionSiteNotifierService service) : BaseComponent, IPersistentEntity, IFinishedStateListener
{
    static readonly ComponentKey SaveKey = new(nameof(ConstructionSiteNotifier));
    static readonly PropertyKey<bool> NotifyKey = new("NotifyOnCompletion");

    public bool NotifyOnCompletion { get; set; }

    public void Load(IEntityLoader entityLoader)
    {
        if (!entityLoader.TryGetComponent(SaveKey, out var s)) { return; }

        if (s.Has(NotifyKey))
        {
            NotifyOnCompletion = s.Get(NotifyKey);
        }
    }

    public void OnEnterFinishedState()
    {
        if (NotifyOnCompletion)
        {
            service.Notify(this);
        }
    }

    public void OnExitFinishedState() { }

    public void Save(IEntitySaver entitySaver)
    {
        if (!NotifyOnCompletion) { return; }

        var s = entitySaver.GetComponent(SaveKey);
        s.Set(NotifyKey, true);
    }
}
