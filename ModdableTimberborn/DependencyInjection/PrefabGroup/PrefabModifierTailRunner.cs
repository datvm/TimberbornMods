namespace ModdableTimberborn.DependencyInjection.PrefabGroup;

sealed class PrefabModifierTailRunner(IEnumerable<IPrefabModifier> modifiers) : IPrefabGroupServiceTailRunner
{
    readonly ImmutableArray<IPrefabModifier> modifiers = [.. modifiers.OrderBy(q => q.Order)];

    public void Run(PrefabGroupService prefabGroupService)
    {
        if (modifiers.Length == 0) { return; }

        var list = prefabGroupService.AllPrefabs.ToArray();
        for (int i = 0; i < list.Length; i++)
        {
            var original = list[i];
            var p = original;

            var prefabSpec = p.GetComponent<PrefabSpec>();
            if (!prefabSpec) { continue; }

            var name = prefabSpec.Name;

            foreach (var m in modifiers)
            {
                if (!m.ShouldModify(name, prefabSpec)) { continue; }

                var copy = Object.Instantiate(p);
                var modified = m.Modify(copy, prefabSpec, original);

                if (modified)
                {
                    ModdableTimberbornUtils.LogVerbose(() => $"Modified prefab '{name}' using '{m.GetType().FullName}'");
                    p = modified;                    
                }
            }

            if (p != original)
            {
                list[i] = p;
            }
        }

        prefabGroupService.AllPrefabs = [.. list];
    }

}
