namespace ScenarioEditor.Components;

public class CustomStartParameters
{
    public bool Enabled { get; set; }

    public int Adult { get; set; } = 8;
    public int Children { get; set; } = 4;
    public int Food { get; set; } = 130;
    public int Water { get; set; } = 0;
}

public class CustomStartComponent : BaseComponent, IPersistentEntity
{
    static readonly ComponentKey SaveKey = new("CustomStart");
    static readonly PropertyKey<bool> EnabledKey = new("Enabled");
    static readonly ListKey<int> StatsKey = new("Stats");

    public CustomStartParameters Parameters { get; } = new();

    public void Load(IEntityLoader entityLoader)
    {
        if (!entityLoader.TryGetComponent(SaveKey, out var s)) { return; }

        var p = Parameters;
        p.Enabled = s.Has(EnabledKey) && s.Get(EnabledKey);

        if (s.Has(StatsKey))
        {
            var stats = s.Get(StatsKey);

            SetIfExists(stats, 0, v => p.Adult = v);
            SetIfExists(stats, 1, v => p.Children = v);
            SetIfExists(stats, 2, v => p.Food = v);
            SetIfExists(stats, 3, v => p.Water = v);
        }

        static void SetIfExists(List<int> arr, int index, Action<int> set)
        {
            if (arr.Count > index)
            {
                set(arr[index]);
            }
        }
    }

    public void Save(IEntitySaver entitySaver)
    {
        var s = entitySaver.GetComponent(SaveKey);
        var p = Parameters;

        if (p.Enabled)
        {
            s.Set(EnabledKey, true);
        }

        s.Set(StatsKey, [p.Adult, p.Children, p.Food, p.Water]);
    }

    public IEnumerable<GoodAmount> InitialGoods => [
        new("Berries", Parameters.Food),
        new("Water", Parameters.Water),
    ];

}
