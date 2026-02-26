namespace MoreAchievements.Components;

[AddTemplateModule2(typeof(Manufactory))]
public class ManufactoryProducedComponent : BaseComponent, IPersistentEntity, IAwakableComponent
{
    static readonly ComponentKey SaveKey = new(nameof(ManufactoryProducedComponent));
    static readonly PropertyKey<bool> HasProducedKey = new("HasProduced");

    public bool HasProduced { get; private set; }

    public void Awake()
    {
        if (!HasProduced)
        {
            GetComponent<Manufactory>().ProductionFinished += OnProductionFinished;
        }
    }

    void OnProductionFinished(object sender, EventArgs e)
    {
        HasProduced = true;
        ((Manufactory)sender).ProductionFinished -= OnProductionFinished;
    }

    public void Load(IEntityLoader entityLoader)
    {
        if (!entityLoader.TryGetComponent(SaveKey, out var s)) { return; }

        if (s.Has(HasProducedKey))
        {
            HasProduced = true;
        }
    }

    public void Save(IEntitySaver entitySaver)
    {
        if (!HasProduced) { return; }

        var s = entitySaver.GetComponent(SaveKey);
        s.Set(HasProducedKey, true);
    }

}
