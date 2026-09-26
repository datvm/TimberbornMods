namespace ConveyorBelt.Services;

[BindSingleton]
public class ConveyorBeltSpeeds(ISpecService specs) : ILoadableSingleton
{
    readonly Dictionary<string, ConveyorBeltSpeedLevel> byId = [];

    public ImmutableArray<ConveyorBeltSpeedLevel> Levels { get; private set; } = [];

    public void Load()
    {
        var spec = specs.GetSpecs<ConveyorBeltSpeedSpec>().SingleOrDefault()
            ?? throw new InvalidOperationException("ConveyorBeltSpeedSpec is missing.");
        if (spec.Levels.Length == 0)
        {
            throw new InvalidOperationException("ConveyorBeltSpeedSpec has no levels.");
        }

        Levels = spec.Levels;
        byId.Clear();
        foreach (var level in Levels)
        {
            if (level.Id.Length == 0)
            {
                throw new InvalidOperationException("ConveyorBeltSpeedSpec level has no Id.");
            }

            byId[level.Id] = level;
        }
    }

    public float ItemsPerHour(string id) => Get(id).ItemsPerHour;

    public string NameKey(string id, string factionId) => Get(id).NameKey(factionId);

    ConveyorBeltSpeedLevel Get(string id)
    {
        if (!byId.TryGetValue(id, out var level))
        {
            throw new InvalidOperationException($"Conveyor belt speed '{id}' is not in ConveyorBeltSpeedSpec.");
        }

        return level;
    }
}
