namespace ScienceShop.Components;

[AddTemplateModule2(typeof(ConsumeScienceManufactorySpec))]
public class ConsumeScienceManufactory(ConsumeScienceService service)
    : BaseComponent, IAwakableComponent, IInitializableEntity, IManufactoryLimiter, IPersistentEntity, IDuplicable<ConsumeScienceManufactory>
{
    static readonly ComponentKey SaveKey = new(nameof(ConsumeScienceManufactory));
    static readonly PropertyKey<int> RemainingKey = new("Remaining");
    static readonly PropertyKey<bool> IndefiniteKey = new("Indefinite");
    static readonly PropertyKey<bool> ScienceTakenKey = new("ScienceTaken");
    static readonly PropertyKey<string> QueueRecipeIdKey = new("QueueRecipeId");

    Manufactory manufactory = null!;
    bool scienceTaken;

    public Manufactory Manufactory => manufactory;
    public RecipeSpec? QueueRecipe { get; private set; }
    public int Remaining { get; private set; }
    public bool Indefinite { get; private set; }
    public RecipeSpec? CurrentRecipe => manufactory.CurrentRecipe;
    public bool HasQueue => Indefinite || Remaining > 0;
    public bool CycleInProgress
        => scienceTaken || manufactory.ProductionProgress > 0f || manufactory._ingredientsConsumed;
    public bool IsRunning => HasQueue || CycleInProgress;
    public bool WaitingForScience
    {
        get
        {
            if (scienceTaken || !IsRunning)
            {
                return false;
            }

            var recipe = manufactory.HasCurrentRecipe ? manufactory.CurrentRecipe : QueueRecipe;
            return service.TryGetRecipe(recipe, out var spec) && !service.HasEnough(spec);
        }
    }
    public ConsumeScienceService Service => service;

    public bool TryGetCurrentSpec([NotNullWhen(true)] out ConsumeScienceRecipeSpec? spec)
        => service.TryGetRecipe(manufactory.CurrentRecipe, out spec);

    public bool WouldReplaceQueue(RecipeSpec recipe) => HasQueue && QueueRecipe != recipe;

    public void Awake()
    {
        manufactory = GetComponent<Manufactory>();
        manufactory.ProductionProgressed += OnProductionProgressed;
        manufactory.ProductionFinished += OnProductionFinished;
        manufactory.RecipeChanged += OnRecipeChanged;
    }

    public void InitializeEntity() => ApplyQueueIfIdle();

    public float ProductionEfficiency()
        => service.ProductionEfficiency(manufactory.CurrentRecipe, IsRunning, scienceTaken);

    public float MaxProductionProgressChange(float expectedProductionProgressChange)
        => ProductionEfficiency() > 0f ? expectedProductionProgressChange : 0f;

    public void Add(RecipeSpec recipe, int cycles)
    {
        if (cycles < 1)
        {
            return;
        }

        if (QueueRecipe == recipe && Indefinite)
        {
            ApplyQueueIfIdle();
            return;
        }

        if (QueueRecipe == recipe)
        {
            Remaining += cycles;
        }
        else
        {
            QueueRecipe = recipe;
            Remaining = cycles;
            Indefinite = false;
        }

        ApplyQueueIfIdle();
    }

    public void AddIndefinite(RecipeSpec recipe)
    {
        QueueRecipe = recipe;
        Remaining = 0;
        Indefinite = true;
        ApplyQueueIfIdle();
    }

    public void ClearQueue()
    {
        QueueRecipe = null;
        Remaining = 0;
        Indefinite = false;
        ApplyQueueIfIdle();
    }

    public void RemoveOne()
    {
        if (Indefinite)
        {
            return;
        }

        if (Remaining <= 1)
        {
            ClearQueue();
            return;
        }

        Remaining--;
        ApplyQueueIfIdle();
    }

    public void ReplaceQueueRecipe(RecipeSpec recipe)
    {
        QueueRecipe = recipe;
        ApplyQueueIfIdle();
    }

    public void SetIndefinite(RecipeSpec recipe, bool indefinite)
    {
        if (indefinite)
        {
            AddIndefinite(recipe);
            return;
        }

        QueueRecipe = recipe;
        Indefinite = false;
        if (Remaining < 1)
        {
            Remaining = 1;
        }

        ApplyQueueIfIdle();
    }

    public void DebugFinishCurrent()
    {
        if (!manufactory.HasCurrentRecipe)
        {
            ApplyQueuedRecipe();
        }

        if (!manufactory.HasCurrentRecipe)
        {
            return;
        }

        manufactory._ingredientsConsumed = true;
        if (!scienceTaken)
        {
            scienceTaken = true;
            ConsumeQueuedCycle();
        }

        manufactory.FinishProduction();
    }

    public void DuplicateFrom(ConsumeScienceManufactory source)
    {
        QueueRecipe = source.QueueRecipe;
        Remaining = source.Remaining;
        Indefinite = source.Indefinite;
    }

    public void Save(IEntitySaver entitySaver)
    {
        if (!HasQueue && !scienceTaken)
        {
            return;
        }

        var s = entitySaver.GetComponent(SaveKey);
        s.Set(RemainingKey, Remaining);
        s.Set(IndefiniteKey, Indefinite);
        s.Set(ScienceTakenKey, scienceTaken);
        if (QueueRecipe is not null)
        {
            s.Set(QueueRecipeIdKey, QueueRecipe.Id);
        }
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
        if (s.Has(QueueRecipeIdKey))
        {
            QueueRecipe = service.FindRecipe(s.Get(QueueRecipeIdKey));
        }
    }

    void ApplyQueueIfIdle()
    {
        if (CycleInProgress)
        {
            return;
        }

        ApplyQueuedRecipe();
    }

    void ApplyQueuedRecipe()
    {
        RecipeSpec? target = HasQueue ? QueueRecipe : null;
        if (manufactory.CurrentRecipe == target)
        {
            return;
        }

        manufactory.SetRecipe(target);
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
        ConsumeQueuedCycle();
    }

    void ConsumeQueuedCycle()
    {
        if (Indefinite || QueueRecipe != manufactory.CurrentRecipe || Remaining < 1)
        {
            return;
        }

        Remaining--;
        if (Remaining == 0 && !Indefinite)
        {
            QueueRecipe = null;
        }
    }

    void OnProductionFinished(object sender, EventArgs e)
    {
        scienceTaken = false;
        ApplyQueuedRecipeNextFrame();
    }

    async void ApplyQueuedRecipeNextFrame()
    {
        await Awaitable.NextFrameAsync();
        if (!this)
        {
            return;
        }

        if (!HasQueue)
        {
            QueueRecipe = null;
            Remaining = 0;
            Indefinite = false;
        }

        if (!HasQueue || QueueRecipe != manufactory.CurrentRecipe)
        {
            ApplyQueuedRecipe();
        }
    }

    void OnRecipeChanged(object sender, EventArgs e)
    {
        scienceTaken = false;
    }
}
