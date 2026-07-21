namespace BuildingRenovations.Services;

[BindSingleton]
public class RenovationRegistry(
    RenovationSpecService renovationSpecs,
    IEnumerable<RenovationBase> renovations
) : ILoadableSingleton
{
    public FrozenDictionary<string, RenovationBase> Renovations { get; private set; } = null!;
    public ImmutableArray<RenovationGroupSpec> OrderedGroups { get; private set; }

    public FrozenDictionary<string, ImmutableArray<RenovationBase>> Groups { get; private set; } = null!;

    public void Load()
    {
        ValidateAndAssignSpecs(renovations);

        Renovations = renovations.ToFrozenDictionary(e => e.Id);

        OrderedGroups = [.. renovationSpecs.Groups.Values.OrderBy(q => q.Order)];

        Groups = renovations
            .GroupBy(e => e.Spec.GroupId)
            .ToFrozenDictionary(
                g => g.Key,
                g => g.OrderBy(e => e.Spec.Order).ToImmutableArray());
    }

    public RenovationBase Get(string id) => Renovations[id];

    public bool TryGet(string id, [NotNullWhen(true)] out RenovationBase? renovation)
        => Renovations.TryGetValue(id, out renovation);

    void ValidateAndAssignSpecs(IEnumerable<RenovationBase> effects)
    {
        Dictionary<string, RenovationBase> byId = [];

        foreach (var e in effects)
        {
            if (!renovationSpecs.Renovations.TryGetValue(e.Id, out var spec))
            {
                throw new Exception($"No renovation spec found for effect id: {e.Id} ({e.GetType().FullName})");
            }

            if (!byId.TryAdd(e.Id, e))
            {
                throw new Exception($"Duplicate renovation effect id: {e.Id} ({e.GetType().FullName}, {byId[e.Id].GetType().FullName})");
            }

            e.Spec = spec;
        }

        if (byId.Count != renovationSpecs.Renovations.Count)
        {
            var missing = renovationSpecs.Renovations.Keys.Except(byId.Keys);
            throw new Exception($"Missing renovation effects for specs: {string.Join(", ", missing)}");
        }
    }
}
