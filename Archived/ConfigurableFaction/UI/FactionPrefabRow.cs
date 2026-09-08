namespace ConfigurableFaction.UI;

public abstract class FactionPrefabRow : FactionItemRow<NormalizedPrefabSpec>
{
#nullable disable
    protected FactionOptions options;
    protected FactionOptionsService optionsService;
#nullable enable

    public virtual FactionPrefabRow SetPrefab(NormalizedPrefabSpec prefab, ILoc t, FactionOptions options, FactionOptionsService optionsService)
    {
        this.options = options;
        this.optionsService = optionsService;

        var label = prefab.PrefabSpec.GetComponentFast<LabeledEntitySpec>();
        var text = GetName(label, t);
        SetItem(prefab, text);

        var existing = ExistingList;
        SetAdditionalFilter((_, isChecked, filter) =>
            !ExistingList.Any(q => q.Path == prefab.Path)
            && (isChecked || !filter.HideSimilar || !existing.Any(q => q.NormalizedName == prefab.NormalizedName)));

        Value = GetList().Contains(prefab.Path);
        OnValueChanged += OnPrefabSelected;

        return this;
    }

    protected virtual string GetName(LabeledEntitySpec spec, ILoc t)
        => t.T(spec.DisplayNameLocKey);

    protected abstract void OnPrefabSelected(NormalizedPrefabSpec spec, bool add);
    protected abstract HashSet<string> GetList();
    protected abstract HashSet<NormalizedPrefabSpec> ExistingList { get; }
}
