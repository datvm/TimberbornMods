namespace ScienceShop.Components;

[AddTemplateModule2(typeof(ConsumeScienceManufactorySpec))]
public class ConsumeScienceManufactory(ConsumeScienceService service)
    : BaseComponent, IAwakableComponent, IManufactoryLimiter, IEntityDescriber, IPersistentEntity, IDuplicable<ConsumeScienceManufactory>
{
    static readonly ComponentKey SaveKey = new(nameof(ConsumeScienceManufactory));
    static readonly PropertyKey<int> RemainingKey = new("Remaining");
    static readonly PropertyKey<bool> IndefiniteKey = new("Indefinite");
    static readonly PropertyKey<bool> ScienceTakenKey = new("ScienceTaken");

    Manufactory manufactory = null!;
    BlockObject blockObject = null!;
    bool scienceTaken;

    public int Remaining { get; private set; }
    public bool Indefinite { get; private set; }
    public bool IsRunning => Indefinite || Remaining > 0;
    public ConsumeScienceService Service => service;
    public RecipeSpec? CurrentRecipe => manufactory.CurrentRecipe;

    public bool TryGetCurrentSpec([NotNullWhen(true)] out ConsumeScienceRecipeSpec? spec)
        => service.TryGetRecipe(manufactory.CurrentRecipe, out spec);

    public void Awake()
    {
        manufactory = GetComponent<Manufactory>();
        blockObject = GetComponent<BlockObject>();
        manufactory.ProductionProgressed += OnProductionProgressed;
        manufactory.ProductionFinished += OnProductionFinished;
        manufactory.RecipeChanged += OnRecipeChanged;
    }

    public float ProductionEfficiency()
        => service.ProductionEfficiency(manufactory.CurrentRecipe, IsRunning, scienceTaken);

    public float MaxProductionProgressChange(float expectedProductionProgressChange)
        => ProductionEfficiency() > 0f ? expectedProductionProgressChange : 0f;

    public void Start(int cycles)
    {
        if (cycles < 1)
        {
            Stop();
            return;
        }

        Remaining = cycles;
        Indefinite = false;
    }

    public void StartIndefinite()
    {
        Remaining = 0;
        Indefinite = true;
    }

    public void Stop()
    {
        Remaining = 0;
        Indefinite = false;
    }

    public IEnumerable<EntityDescription> DescribeEntity()
    {
        if (!blockObject.IsPreview)
        {
            yield break;
        }

        foreach (var description in service.Describe(manufactory))
        {
            yield return description;
        }
    }

    public void DuplicateFrom(ConsumeScienceManufactory source)
    {
        Remaining = source.Remaining;
        Indefinite = source.Indefinite;
    }

    public void Save(IEntitySaver entitySaver)
    {
        if (!IsRunning && !scienceTaken)
        {
            return;
        }

        var s = entitySaver.GetComponent(SaveKey);
        s.Set(RemainingKey, Remaining);
        s.Set(IndefiniteKey, Indefinite);
        s.Set(ScienceTakenKey, scienceTaken);
    }

    public void Load(IEntityLoader entityLoader)
    {
        if (!entityLoader.TryGetComponent(SaveKey, out var s))
        {
            return;
        }

        Remaining = s.Has(RemainingKey) ? s.Get(RemainingKey) : 0;
        Indefinite = s.Has(IndefiniteKey) && s.Get(IndefiniteKey);
        scienceTaken = s.Has(ScienceTakenKey) && s.Get(ScienceTakenKey);
    }

    void OnProductionProgressed(object sender, ProductionProgressedEventArgs e)
    {
        if (scienceTaken || !manufactory._ingredientsConsumed)
        {
            return;
        }

        if (!TryGetCurrentSpec(out var spec))
        {
            return;
        }

        service.TakeScience(spec);
        scienceTaken = true;
    }

    void OnProductionFinished(object sender, EventArgs e)
    {
        scienceTaken = false;
        if (Indefinite)
        {
            return;
        }

        if (Remaining > 0)
        {
            Remaining--;
        }
    }

    void OnRecipeChanged(object sender, EventArgs e)
    {
        scienceTaken = false;
        Stop();
    }
}
