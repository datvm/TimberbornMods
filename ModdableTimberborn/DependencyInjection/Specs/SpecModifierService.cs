namespace ModdableTimberborn.DependencyInjection;

public class SpecModifierService(
    IEnumerable<ISpecModifier> modifiers,
    BlueprintSourceService blueprintSourceService
) : ISpecServiceTailRunner
{
    readonly FrozenDictionary<Type, ImmutableArray<ISpecModifier>> modifiersByTypes = modifiers.GroupToDictionary(
        q => q.Type,
        q => q.OrderBy(q => q.Order));

    public void Run(SpecService specService)
    {
        if (modifiersByTypes.Count == 0) { return; }

        var cachedBp = specService._cachedBlueprintsBySpecs;
        foreach (var (type, modifiers) in modifiersByTypes)
        {
            if (!cachedBp.TryGetValue(type, out var lazies)) { continue; }

            foreach (var b in lazies)
            {
                var v = b.Value;
                if (!v.HasSpec<TemplateCollectionSpec>()) { continue; }

                var s = v.GetSpec<TemplateCollectionSpec>();
            }

            var bps = lazies.Select(q => new EditableBlueprint(q.Value)).ToArray();
            foreach (var m in modifiers)
            {
                if (!m.ShouldRun) { continue; }

                bps = [.. m.Modify(bps)];
            }

            const string SourceName = $"{nameof(ModdableTimberborn)}.{nameof(SpecModifierService)}";
            List<Lazy<Blueprint>> dst = [];
            foreach (var bp in bps)
            {
                var actual = bp.ToBlueprint();
                dst.Add(new(() => actual));

                var bpFileSrc = bp.Source ??= new(bp.Name, SourceName, ["{}"], [SourceName]);
                bpFileSrc = bpFileSrc.AddJson("{}", SourceName);

                blueprintSourceService.Add(bp, bpFileSrc);
            }

            cachedBp[type] = dst;
        }
    }

}
