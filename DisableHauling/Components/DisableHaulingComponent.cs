namespace DisableHauling.Components;

[AddTemplateModule2(typeof(HaulCandidate))]
public class DisableHaulingComponent : BaseComponent, IPersistentEntity, IDuplicable<DisableHaulingComponent>
{
    static readonly ComponentKey SaveKey = new(nameof(DisableHaulingComponent));
    static readonly PropertyKey<bool> DisableHaulingKey = new("DisableHauling");

    public bool DisableHauling { get; set; }

    public void DuplicateFrom(DisableHaulingComponent source)
    {
        DisableHauling = source.DisableHauling;
    }

    public void Load(IEntityLoader entityLoader)
    {
        if (!entityLoader.TryGetComponent(SaveKey, out var s)) { return; }
        DisableHauling = s.Has(DisableHaulingKey) && s.Get(DisableHaulingKey);
    }

    public void Save(IEntitySaver entitySaver)
    {
        var s = entitySaver.GetComponent(SaveKey);
        s.Set(DisableHaulingKey, DisableHauling);
    }
}
