namespace BuildingRenovations.Services;

[BindSingleton]
public class RenovationRegistry(
    RenovationSpecService renovationSpecs,
    IEnumerable<RenovationBase> renovations,
    FactionService factionService,
    IGoodService goodService,
    TemplateService templateService
) : ILoadableSingleton
{
    public readonly FactionService FactionService = factionService;
    public readonly IGoodService GoodService = goodService;
    public readonly TemplateService TemplateService = templateService;

    public FrozenDictionary<string, RenovationBase> Renovations { get; private set; } = null!;
    public ImmutableArray<RenovationGroupSpec> OrderedGroups { get; private set; }

    public FrozenDictionary<string, ImmutableArray<RenovationBase>> Groups { get; private set; } = null!;

    public void Load()
    {
        ValidateAndAssignSpecs();
        OrderedGroups = [.. renovationSpecs.Groups.Values.OrderBy(q => q.Order)];

        Groups = Renovations.Values
            .GroupBy(e => e.Spec.GroupId)
            .ToFrozenDictionary(
                g => g.Key,
                g => g.OrderBy(e => e.Spec.Order).ToImmutableArray());
    }

    public RenovationBase Get(string id) => Renovations[id];

    public bool TryGet(string id, [NotNullWhen(true)] out RenovationBase? renovation)
        => Renovations.TryGetValue(id, out renovation);

    public bool ContainsAllGoods(ImmutableArray<GoodAmountSpec> goodIds)
        => ContainsAllGoods(goodIds.Select(g => g.Id));

    public bool ContainsAllGoods(IEnumerable<string> goodIds)
    {
        foreach (var id in goodIds)
        {
            if (id != RenovationHelpers.ScienceId && !GoodService.HasGood(id))
            {
                return false;
            }
        }

        return true;
    }

    public bool HasAnyBuildingWith<T>() where T : ComponentSpec // Component is not available at this point yet, only Specs
        => TemplateService.GetAll<T>().Any();

    void ValidateAndAssignSpecs()
    {
        Dictionary<string, RenovationBase> byId = [];
        var factionId = FactionService.Current.Id;

        foreach (var e in renovations)
        {
            if (!renovationSpecs.Renovations.TryGetValue(e.Id, out var spec))
            {
                throw new Exception($"No {nameof(RenovationSpec)} found with Id: {e.Id} (from effect {e.GetType().FullName})");
            }

            if (spec.CustomCodeCost)
            {
                if (e is not ICustomCostRenovation)
                {
                    throw new Exception($"Renovation effect {e.GetType().FullName} has {nameof(spec.CustomCodeCost)} set to true, but does not implement {nameof(ICustomCostRenovation)}");
                }
            }
            else
            {
                if (e is ICustomCostRenovation)
                {
                    throw new Exception($"Renovation effect {e.GetType().FullName} implements {nameof(ICustomCostRenovation)}, but {nameof(spec.CustomCodeCost)} is false");
                }
            }

            e.Spec = spec;
            if (!e.IsAvailableToThisGame(this))
            {
                RenovationHelpers.LogVerbose(() => $"Renovation {e.Id} is not available for this game.");
                continue;
            }

            if (!byId.TryAdd(e.Id, e))
            {
                throw new Exception($"Duplicate renovation effect id: {e.Id} ({e.GetType().FullName}, {byId[e.Id].GetType().FullName})");
            }
        }

        Renovations = byId.ToFrozenDictionary();
    }
}
