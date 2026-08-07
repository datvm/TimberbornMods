namespace ConstructionSiteHauler.Components;

[AddTemplateModule2(typeof(ConstructionSite))]
public class ConstructionSiteBuilderHaulingSettings
    : BaseComponent, IPersistentEntity, IDuplicable<ConstructionSiteBuilderHaulingSettings>
{
    static readonly ComponentKey SaveKey = new(nameof(ConstructionSiteBuilderHaulingSettings));
    static readonly PropertyKey<bool> DisableBuilderHaulingKey = new("DisableBuilderHauling");

    public bool DisableBuilderHauling { get; set; }

    public void DuplicateFrom(ConstructionSiteBuilderHaulingSettings source)
    {
        DisableBuilderHauling = source.DisableBuilderHauling;
    }

    public void Load(IEntityLoader entityLoader)
    {
        if (!entityLoader.TryGetComponent(SaveKey, out var s))
        {
            return;
        }

        DisableBuilderHauling = s.Has(DisableBuilderHaulingKey) && s.Get(DisableBuilderHaulingKey);
    }

    public void Save(IEntitySaver entitySaver)
    {
        if (!DisableBuilderHauling)
        {
            return;
        }

        entitySaver.GetComponent(SaveKey).Set(DisableBuilderHaulingKey, DisableBuilderHauling);
    }
}
