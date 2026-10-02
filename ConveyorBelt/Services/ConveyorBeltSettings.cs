namespace ConveyorBelt.Services;

[BindSingleton]
public class ConveyorBeltSettings(ISpecService specs) : ILoadableSingleton
{
    float[] itemsPerHour = [];
    FrozenDictionary<string, int> index = FrozenDictionary<string, int>.Empty;

    public int Count => itemsPerHour.Length;

    public void Load()
    {
        var spec = specs.GetSpecs<ConveyorBeltSettingsSpec>().SingleOrDefault()
            ?? throw new InvalidOperationException("ConveyorBeltSettingsSpec is missing.");
        BeltRates.Use(spec.Capacity);
        if (spec.Levels.Length == 0)
        {
            throw new InvalidOperationException("ConveyorBeltSettingsSpec has no levels.");
        }

        itemsPerHour = new float[spec.Levels.Length];
        var map = new Dictionary<string, int>(spec.Levels.Length);
        for (var i = 0; i < spec.Levels.Length; i++)
        {
            var level = spec.Levels[i];
            itemsPerHour[i] = level.ItemsPerHour;
            map[level.Id] = i;
        }

        index = map.ToFrozenDictionary();
    }

    public int Index(string id)
    {
        if (!index.TryGetValue(id, out var slot))
        {
            throw new InvalidOperationException($"Conveyor belt speed '{id}' is not in ConveyorBeltSettingsSpec.");
        }

        return slot;
    }

    public float ItemsPerHour(int speed) => itemsPerHour[speed];
}
