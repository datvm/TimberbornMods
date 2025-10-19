namespace ColorfulZipline.Services;

public class ZiplineColoringService(
    ZiplineCableRenderer renderer,
    ISingletonLoader loader,
    EntityRegistry entityRegistry
) : ISaveableSingleton, ILoadableSingleton
{
    static readonly SingletonKey SaveKey = new(nameof(ZiplineColoringService));
    static readonly ListKey<string> ColorListKey = new("CableColors");

    readonly Dictionary<ZiplineTowerIdPair, ZiplineCableColor> cableColors = [];

    public ZiplineCableColor GetColor(ZiplineTowerIdPair pair) 
        => cableColors.TryGetValue(pair, out var color) ? color : ZiplineCableColor.Default;

    public void SetColor(CableKey pair, ZiplineCableColor color)
    {
        cableColors[pair] = color;
        ApplyColor(pair, color);
    }

    public void ApplyColor(CableKey pair)
    {
        ApplyColor(pair, GetColor(pair));
    }

    void ApplyColor(CableKey pair, ZiplineCableColor color)
    {
        if (!renderer.TryGetCableModel(pair.ZiplineTower, pair.OtherZiplineTower, out var model, out _)) { return; }

        model._leftMeshRenderer.material.color = color.Left;
        model._rightMeshRenderer.material.color = color.Right;
    }

    public void Save(ISingletonSaver singletonSaver)
    {
        CleanupRemovedTowers();

        var s = singletonSaver.GetSingleton(SaveKey);

        s.Set(ColorListKey, [..cableColors.Select(kvp =>
        {
            var (key, color) = kvp;

            return $"{key.TowerId1}|{key.TowerId2}|{ColorUtility.ToHtmlStringRGBA(color.Left)}|{ColorUtility.ToHtmlStringRGBA(color.Right)}";
        })]);
    }

    void CleanupRemovedTowers()
    {
        Dictionary<Guid, bool> exist = [];

        foreach (var pair in cableColors.Keys.ToArray())
        {
            var id1 = pair.TowerId1;
            var id2 = pair.TowerId2;

            if (!GetOrAdd(id1) || !GetOrAdd(id2))
            {
                cableColors.Remove(pair);
            }
        }

        bool GetOrAdd(Guid id)
        {
            if (exist.TryGetValue(id, out var e)) { return e; }

            bool exists = entityRegistry.GetEntity(id);
            exist[id] = exists;
            return exists;
        }
    }

    public void Load()
    {
        LoadSavedData();
    }

    void LoadSavedData()
    {
        if (!loader.TryGetSingleton(SaveKey, out var s) || !s.Has(ColorListKey)) { return; }

        var entries = s.Get(ColorListKey);
        foreach (var entry in entries)
        {
            var parts = entry.Split('|');
            if (parts.Length != 4) { continue; }

            var towerId1 = parts[0];
            var towerId2 = parts[1];
            if (!ColorUtility.TryParseHtmlString($"#{parts[2]}", out var leftColor)) { leftColor = Color.white; }
            if (!ColorUtility.TryParseHtmlString($"#{parts[3]}", out var rightColor)) { rightColor = Color.white; }

            var pair = new ZiplineTowerIdPair(Guid.Parse(towerId1), Guid.Parse(towerId2));
            var cableColor = new ZiplineCableColor(leftColor, rightColor);
            cableColors[pair] = cableColor;
        }
    }

}

public readonly record struct ZiplineCableColor(Color Left, Color Right)
{
    public static readonly ZiplineCableColor Default = new(Color.white, Color.white);
}

public readonly struct ZiplineTowerIdPair
{
    public Guid TowerId1 { get; }
    public Guid TowerId2 { get; }

    public ZiplineTowerIdPair(Guid id1, Guid id2)
    {
        if (id1.CompareTo(id2) <= 0)
        {
            TowerId1 = id1;
            TowerId2 = id2;
        }
        else
        {
            TowerId1 = id2;
            TowerId2 = id1;
        }
    }

    public ZiplineTowerIdPair(ZiplineTower tower1, ZiplineTower tower2)
        : this(GetId(tower1), GetId(tower2)) { }

    public static implicit operator ZiplineTowerIdPair(CableKey cableKey) => new(
        GetId(cableKey.ZiplineTower),
        GetId(cableKey.OtherZiplineTower));

    public static Guid GetId<T>(T comp) where T : BaseComponent
        => comp.GetComponentFast<EntityComponent>().EntityId;

}