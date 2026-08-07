namespace ConstructionSiteHauler.Components;

[AddTemplateModule2(typeof(BuilderHubWorkplaceBehavior))]
public class BuilderHubHaulingDisabler : BaseComponent, IPersistentEntity, IDuplicable<BuilderHubHaulingDisabler>
{
    static readonly ComponentKey SaveKey = new(nameof(BuilderHubHaulingDisabler));
    static readonly PropertyKey<bool> DisableHaulingMaterialsKey = new("DisableHaulingMaterials");

    public bool DisableHaulingMaterials { get; set; }

    public void Load(IEntityLoader entityLoader)
    {
        if (!entityLoader.TryGetComponent(SaveKey, out var s)) { return; }

        DisableHaulingMaterials = s.Get(DisableHaulingMaterialsKey);
    }

    public void Save(IEntitySaver entitySaver)
    {
        var s = entitySaver.GetComponent(SaveKey);
        s.Set(DisableHaulingMaterialsKey, DisableHaulingMaterials);
    }

    public void DuplicateFrom(BuilderHubHaulingDisabler source)
    {
        DisableHaulingMaterials = source.DisableHaulingMaterials;
    }

}
