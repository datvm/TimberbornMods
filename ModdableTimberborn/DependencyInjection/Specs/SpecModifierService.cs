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

                var bps = ConvertFromLazies(curr);
                var modified = m.Modify(bps);
                curr = ConvertToLazies(modified);
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

    public static List<Blueprint> ConvertFromLazies(IEnumerable<Lazy<Blueprint>> lazies) => [.. lazies.Select(q => q.Value)];
    public static List<Lazy<Blueprint>> ConvertToLazies(IEnumerable<Blueprint> bps) => [.. bps.Select(q => new Lazy<Blueprint>(() => q))];

}
