namespace BuildingHP.Services;

public class RenovationRegistry(
    RenovationSpecService renovationSpec,
    IEnumerable<IRenovationProvider> providers
) : ILoadableSingleton
{
    readonly RenovationSpecService renovationSpec = renovationSpec;

    public RenovationSpecService RenovationSpecService { get; } = renovationSpec;
    public FrozenDictionary<string, IRenovationProvider> Providers { get; private set; } = FrozenDictionary<string, IRenovationProvider>.Empty;

    public ImmutableArray<RenovationGroupSpec> OrderedGroups { get; private set; } = [];
    public FrozenDictionary<string, ImmutableArray<IRenovationProvider>> ProviderGroups { get; private set; } = FrozenDictionary<string, ImmutableArray<IRenovationProvider>>.Empty;

    public void Load()
    {
        ValidateAndAssignSpecs();

        OrderedGroups = [.. renovationSpec.Groups.Values.OrderBy(q => q.Order)];
        ProviderGroups = Providers.Values
            .GroupBy(q => q.RenovationSpec.GroupId)
            .ToFrozenDictionary(
                g => g.Key,
                g => g.OrderBy(p => p.RenovationSpec.Order).ToImmutableArray());
    }

    void ValidateAndAssignSpecs()
    {
        Providers = providers.ToFrozenDictionary(q => q.Id);
        Dictionary<string, IRenovationProvider> providersBySpec = [];

        foreach (var p in providers)
        {
            var id = p.Id;
            if (!renovationSpec.Renovations.TryGetValue(id, out var spec))
            {
                throw new Exception($"No renovation spec found for provider id: {p.Id} ({p.GetType().FullName})");
            }
            p.RenovationSpec = spec;
            
            if (providersBySpec.TryGetValue(id, out var existing))
            {
                throw new Exception($"Duplicate renovation provider id: {p.Id} ({existing.GetType().FullName}, {p.GetType().FullName})");
            }
            providersBySpec[id] = p;
        }

        if (providersBySpec.Count != renovationSpec.Renovations.Count)
        {
            var missingSpecs = renovationSpec.Renovations.Keys.Except(providersBySpec.Keys);
            throw new Exception($"Missing renovation providers for specs: {string.Join(", ", missingSpecs)}");
        }
    }

}
