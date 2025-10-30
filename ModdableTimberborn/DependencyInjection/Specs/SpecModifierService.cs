namespace ModdableTimberborn.DependencyInjection;

public class SpecModifierService(
    IEnumerable<ISpecModifier> modifiers
) : ISpecServiceTailRunner
{
    readonly FrozenDictionary<Type, ImmutableArray<ISpecModifier>> modifiersByTypes = modifiers.GroupToDictionary(
        q => q.Type,
        q => q.OrderBy(q => q.Order));

    public void Run(SpecService specService)
    {
        if (modifiersByTypes.Count == 0) { return; }

        var cachedBp = specService._cachedBlueprints;
        foreach (var (type, modifiers) in modifiersByTypes)
        {
            if (!cachedBp.TryGetValue(type, out var lazies)) { continue; }

            var bps = lazies.Select(q => new EditableBlueprint(q.Value)).ToArray();
            foreach (var m in modifiers)
            {
                bps = [.. m.Modify(bps)];
            }

            cachedBp[type] = [.. bps.Select(q => new Lazy<Blueprint>(() => q.ToBlueprint()))];
        }
    }

}
