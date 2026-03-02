namespace BuildingBlueprints.Services;

[BindSingleton]
public class BlueprintListingSettings(ISingletonLoader loader) : ILoadableSingleton, ISaveableSingleton
{
    static readonly SingletonKey SaveKey = new(nameof(BlueprintListingSettings));
    static readonly PropertyKey<bool> HideInvalidsKey = new("HideInvalids");
    static readonly PropertyKey<bool> ShowLocalKey = new("ShowLocal");
    static readonly PropertyKey<bool> ShowNonLocalKey = new("ShowNonLocal");
    static readonly PropertyKey<bool> ExpandFilterKey = new("ExpandFilter");
    static readonly ListKey<string> CollapsingTagsKey = new("CollapsingTags");

    public bool HideInvalids { get; set; } = true;
    public bool ExpandFilter { get; set; } = true;
    public string FilterKeyword { get; set; } = "";
    public bool ShowLocal { get; set; } = true;
    public bool ShowNonLocal { get; set; } = true;
    public HashSet<string> FilteredTags { get; } = [];
    public HashSet<string> CollapsingTags { get; } = [];

    public void Load()
    {
        if (!loader.TryGetSingleton(SaveKey, out var s)) { return; }

        HideInvalids = s.Has(HideInvalidsKey);
        ShowLocal = s.Has(ShowLocalKey);
        ShowNonLocal = s.Has(ShowNonLocalKey);
        ExpandFilter = s.Has(ExpandFilterKey);

        if (s.Has(CollapsingTagsKey))
        {
            CollapsingTags.Clear();
            CollapsingTags.AddRange(s.Get(CollapsingTagsKey));
        }
    }

    public void Save(ISingletonSaver singletonSaver)
    {
        var s = singletonSaver.GetSingleton(SaveKey);

        if (HideInvalids) { s.Set(HideInvalidsKey, true); }
        if (ShowLocal) { s.Set(ShowLocalKey, true); }
        if (ShowNonLocal) { s.Set(ShowNonLocalKey, true); }
        if (ExpandFilter) { s.Set(ExpandFilterKey, true); }
        if (CollapsingTags.Count > 0) { s.Set(CollapsingTagsKey, CollapsingTags); }
    }

}
