namespace ModdableTimberborn.DependencyInjection.Specs;

public class SpecModifierService(
    IEnumerable<ISpecModifier> modifiers
) : ISpecServiceTailRunner
{
    readonly ImmutableArray<ISpecModifier> modifiers = [.. modifiers.OrderBy(q => q.Order)];

    public void Run(SpecService specService)
    {
        if (modifiers.Length == 0) { return; }

        Dictionary<Type, List<Lazy<Blueprint>>> replacing = [];

        foreach (var (t, lazies) in specService._cachedBlueprints)
        {
            var curr = lazies;

            foreach (var m in modifiers)
            {
                if (t != m.Type) { continue; }

                var bps = curr.Select(q => q.Value).ToList();
                var modified = m.Modify(bps);
                curr = [.. modified.Select(q => new Lazy<Blueprint>(() => q))];
            }

            if (curr != lazies)
            {
                replacing[t] = curr;
            }
        }

        foreach (var (t, lazies) in replacing)
        {
            specService._cachedBlueprints[t] = lazies;
        }
    }
}
