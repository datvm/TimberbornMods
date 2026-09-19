namespace GlobalInventory.Services;

[BindSingleton]
public class GlobalGoodSpecService(ISpecService specs, IGoodService goods) : ILoadableSingleton
{
    FrozenDictionary<string, GlobalGoodSpec> byId = FrozenDictionary<string, GlobalGoodSpec>.Empty;
    public IReadOnlyList<GlobalGoodSpec> All { get; private set; } = [];

    public void Load()
    {
        Dictionary<string, GlobalGoodSpec> map = [];
        foreach (var spec in specs.GetSpecs<GlobalGoodSpec>())
        {
            var resolved = Resolve(spec);
            Validate(resolved);
            if (!map.TryAdd(resolved.Id, resolved))
            {
                var existing = map[resolved.Id];
                throw new InvalidOperationException(
                    $"Duplicate GlobalGoodSpec Id '{resolved.Id}' from '{resolved.Blueprint.Name}' and '{existing.Blueprint.Name}'.");
            }
        }

        byId = map.ToFrozenDictionary();
        All = [.. map.Values
            .OrderBy(q => q.Order)
            .ThenBy(q => q.DisplayName.Value, StringComparer.CurrentCultureIgnoreCase)];
    }

    public GlobalGoodSpec Get(string id)
    {
        if (TryGet(id, out var spec))
        {
            return spec;
        }

        throw new ArgumentException($"GlobalGoodSpec with id '{id}' not found.");
    }

    public bool TryGet(string id, [NotNullWhen(true)] out GlobalGoodSpec? spec) => byId.TryGetValue(id, out spec);

    GlobalGoodSpec Resolve(GlobalGoodSpec spec)
    {
        if (spec.CopiedGoodId is null)
        {
            return spec;
        }

        var good = goods.GetGoodOrNull(spec.CopiedGoodId)
            ?? throw new InvalidOperationException(
                $"GlobalGoodSpec in blueprint '{spec.Blueprint.Name}' CopyFromGoodSpec could not find GoodSpec '{spec.CopiedGoodId}'.");

        return spec with
        {
            Id = good.Id,
            DisplayName = good.DisplayName,
            PluralDisplayName = good.PluralDisplayName,
            Icon = good.Icon,
            IconFlipped = good.IconFlipped,
            IconSmall = good.IconSmall,
        };
    }

    void Validate(GlobalGoodSpec spec)
    {
        if (string.IsNullOrEmpty(spec.Id))
        {
            throw new InvalidOperationException($"GlobalGoodSpec Id is empty ({spec.Blueprint.Name}).");
        }

        if (spec.MaxCapacity is float max && max < spec.MinCapacity)
        {
            throw new InvalidOperationException(
                $"GlobalGoodSpec '{spec.Id}' MaxCapacity {max} is below MinCapacity {spec.MinCapacity}.");
        }

        if (spec.InitialAmount < spec.MinCapacity
            || (spec.MaxCapacity is float cap && spec.InitialAmount > cap))
        {
            throw new InvalidOperationException(
                $"GlobalGoodSpec '{spec.Id}' InitialAmount {spec.InitialAmount} is outside capacity.");
        }

        if (string.IsNullOrEmpty(spec.DisplayName.Value))
        {
            throw new InvalidOperationException(
                $"GlobalGoodSpec '{spec.Id}' is missing a display name.");
        }

        if ((spec.Icon is null || string.IsNullOrEmpty(spec.Icon.Path))
            && spec.IconSmall?.Value is null)
        {
            throw new InvalidOperationException(
                $"GlobalGoodSpec '{spec.Id}' is missing an icon.");
        }
    }
}
