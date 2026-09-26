namespace ConveyorBelt.Specs;

public record ConveyorBeltSpeedSpec : ComponentSpec
{
    [Serialize]
    public ImmutableArray<ConveyorBeltSpeedLevel> Levels { get; init; } = [];
}

public record ConveyorBeltSpeedLevel
{
    [Serialize]
    public string Id { get; init; } = "";

    [Serialize]
    public float ItemsPerHour { get; init; }

    [Serialize]
    public string NameLocKey { get; init; } = "";

    [Serialize]
    public string IronTeethNameLocKey { get; init; } = "";

    public string NameKey(string factionId)
    {
        if (factionId == "IronTeeth" && IronTeethNameLocKey.Length > 0)
        {
            return IronTeethNameLocKey;
        }

        return NameLocKey;
    }
}
