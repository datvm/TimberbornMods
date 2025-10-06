namespace ModdableTimberborn.DependencyInjection.PrefabGroup;

sealed class PrefabModifierTailRunner(IEnumerable<IPrefabModifier> modifiers) : IPrefabGroupServiceTailRunner
{
    readonly ImmutableArray<IPrefabModifier> modifiers = [.. modifiers.OrderBy(q => q.Order)];

    public void Run(PrefabGroupService prefabGroupService)
    {
        if (modifiers.Length == 0) { return; }

        var prefabObj = new GameObject();
        prefabObj.SetActive(false);
        var prefabObjT = prefabObj.transform;

        var list = prefabGroupService.AllPrefabs.ToArray();
        List<int> changes = [];
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

                var copy = Object.Instantiate(p, prefabObjT);
                var copiedPrefabSpec = copy.GetComponent<PrefabSpec>();
                var modified = m.Modify(copy, copiedPrefabSpec, original);

                if (modified)
                {
                    ModdableTimberbornUtils.LogVerbose(() => $"Modified prefab '{name}' using '{m.GetType().FullName}'");
                    if (p != original)
                    {
                        Object.Destroy(p);
                    }
                    p = modified;
                }
                else
                {
                    Object.Destroy(copy);
                }
            }

            if (p != original)
            {
                list[i] = p;
                changes.Add(changes.Count);
            }
        }

        if (changes.Count == 0) { return; }
        prefabGroupService.AllPrefabs = [.. list];
    }

}
