namespace BlueprintRelics.Services;

[BindSingleton]
public class BlueprintRelicRecipeUpgradeService(
    ISingletonLoader loader,
    ISpecService specs,
    LiveRecipeModifierService recipeModifier,
    RecipeSpecService recipeSpecService,
    ModdableRecipeLockService locker,
    TemplateService templateService
) : ILoadableSingleton, ISaveableSingleton
{
    static readonly SingletonKey SaveKey = new(nameof(BlueprintRelicRecipeUpgradeService));
    static readonly ListKey<RecipeUpgradeRecord> UpgradesList = new("RecipeUpgrades");

    readonly Dictionary<string, RecipeUpgradeRecord> upgrades = [];
    public IReadOnlyDictionary<string, RecipeUpgradeRecord> Upgrades => upgrades;

    public BlueprintRelicRecipeUpgradeSpec UpgradeSpec { get; private set; } = null!;
    ImmutableArray<string> allRecipes = [];

    public void Load()
    {
        UpgradeSpec = specs.GetSingleSpec<BlueprintRelicRecipeUpgradeSpec>();
        LoadSavedData();
        ApplyLoadingTimeUpgrades();

        GetAllRecipes();
    }

    void GetAllRecipes()
    {
        var manufactories = templateService.GetAll<ManufactorySpec>();
        allRecipes = [.. manufactories.SelectMany(q => q.ProductionRecipeIds).Distinct()];
    }

    public RecipeUpgradeRecord GetOrCreate(string id) => upgrades.GetOrAdd(id, () => new(id));

    public RecipeSpec GetRandomUnlockedRecipe()
    {
        while (true)
        {
            var id = allRecipes[Random.RandomRangeInt(0, allRecipes.Length)];
            if (!locker.IsLocked(id, out _))
            {
                return recipeSpecService.GetRecipe(id);
            }
        }
    }

    public void UpgradeCapacity(string recipeId)
    {
        var record = GetOrCreate(recipeId);
        record.CapacityUpgrades++;

        var rep = recipeSpecService.GetRecipe(recipeId);
        recipeModifier.Modify(recipeId, curr => curr with
        {
            CyclesCapacity = curr.CyclesCapacity + GetNextAdditionalCapacity(curr.CyclesCapacity)
        });
    }
    public int GetNextAdditionalCapacity(RecipeSpec spec) => GetNextAdditionalCapacity(spec.CyclesCapacity);
    int GetNextAdditionalCapacity(int currCapacity) => Mathf.Max(1, Mathf.FloorToInt(currCapacity * UpgradeSpec.CapacityIncrease));

    public void UpgradeTimeReduction(string recipeId)
    {
        var record = GetOrCreate(recipeId);
        record.TimeReductionUpgrades++;

        recipeModifier.Modify(recipeId, curr => curr with
        {
            CycleDurationInHours = GetNextTimeAfterReduction(curr.CycleDurationInHours)
        });
    }
    public float GetNextTimeAfterReduction(RecipeSpec spec) => GetNextTimeAfterReduction(spec.CycleDurationInHours);
    float GetNextTimeAfterReduction(float currTime) => Math.Max(.01f, currTime * (1f - UpgradeSpec.TimeReduction));

    public void UpgradeOutput(string recipeId, string outputId)
    {
        var record = GetOrCreate(recipeId);
        record.OutputUpgrades[outputId] = record.OutputUpgrades.GetValueOrDefault(outputId) + 1;

        recipeModifier.Modify(recipeId, curr =>
        {
            var index = FindOutputIndex(curr, outputId);

            return curr with
            {
                Products = [..curr.Products.Select((p, i) => i == index
                    ? p with { Amount = p.Amount + UpgradeSpec.AdditionalOutput }
                    : p
                )]
            };
        });
    }

    public IEnumerable<GoodAmountSpec> GetOutputUpgradeInfo(RecipeSpec spec, string outputId)
    {
        var index = FindOutputIndex(spec, outputId);

        return spec.Products
            .Select((p, i) => i == index
                ? p with { Amount = p.Amount + UpgradeSpec.AdditionalOutput }
                : p
            );
    }
    int FindOutputIndex(RecipeSpec spec, string outputId)
    {
        for (int i = 0; i < spec.Products.Length; i++)
        {
            if (spec.Products[i].Id == outputId)
            {
                return i;
            }
        }

        throw new InvalidOperationException($"Output '{outputId}' not found in recipe '{spec.Id}' products.");
    }

    void LoadSavedData()
    {
        if (!loader.TryGetSingleton(SaveKey, out var s)) { return; }

        if (s.Has(UpgradesList))
        {
            foreach (var r in s.Get(UpgradesList, RecipeUpgradeRecord.Serializer))
            {
                upgrades[r.Id] = r;
            }
        }
    }

    void ApplyLoadingTimeUpgrades()
    {
        foreach (var (id, curr) in upgrades)
        {
            if (recipeSpecService.GetRecipe(id) is null)
            {
                Debug.LogWarning($"[{nameof(BlueprintRelics)}] Recipe '{id}' no longer exists, skipping upgrades.");
                continue;
            }

            recipeModifier.Modify(id, original =>
            {
                var capacity = original.CyclesCapacity;
                if (curr.CapacityUpgrades > 0)
                {
                    for (int i = 0; i < curr.CapacityUpgrades; i++)
                    {
                        capacity += GetNextAdditionalCapacity(capacity);
                    }
                }

                var time = original.CycleDurationInHours;
                if (curr.TimeReductionUpgrades > 0)
                {
                    time = Math.Max(.01f, time * Mathf.Pow((float)(1f - UpgradeSpec.TimeReduction), curr.TimeReductionUpgrades));
                }

                var outputs = original.Products.ToDictionary(q => q.Id, q => q.Amount);
                if (curr.OutputUpgrades.Count > 0)
                {
                    foreach (var (outputId, upgradeCount) in curr.OutputUpgrades)
                    {
                        if (outputs.TryGetValue(outputId, out var amount))
                        {
                            outputs[outputId] = amount + upgradeCount * UpgradeSpec.AdditionalOutput;
                        }
                    }
                }

                return original with
                {
                    CyclesCapacity = capacity,
                    CycleDurationInHours = time,
                    Products = [..outputs.Select(kv => new GoodAmountSpec()
                    {
                        Id = kv.Key,
                        Amount = kv.Value
                    })],
                };
            });
        }
    }

    public void Save(ISingletonSaver singletonSaver)
    {
        var s = singletonSaver.GetSingleton(SaveKey);
        s.Set(UpgradesList, upgrades.Values, RecipeUpgradeRecord.Serializer);
    }
}
