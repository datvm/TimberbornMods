namespace ModdableWeathers.WeatherModifiers;

#nullable disable
public record ModdableWeatherModifierSpec : ComponentSpec
{
    [Serialize]
    public string Id { get; init; }

    [Serialize]
    public int Order { get; init; }

    [Serialize]
    public string NameLoc { get; init; }
    [Serialize(nameof(NameLoc))]
    public LocalizedText Name { get; init; }

    [Serialize]
    public string DescLoc { get; init; }
    [Serialize(nameof(DescLoc))]
    public LocalizedText Description { get; init; }

    [Serialize]
    public ImmutableArray<ModdableWeatherModifierCompatibilitySpec> CompatibleWeathers { get; set; } = [];

    [Serialize]
    public ImmutableArray<string> IncompatibleModifierIds { get; init; } = [];

}

public record ModdableWeatherModifierCompatibilitySpec
{

    [Serialize]
    public string WeatherId { get; init; }

    [Serialize]
    public int Chance { get; init; }

    [Serialize]
    public int StartCycle { get; init; }

    [Serialize]
    public bool DefaultEnabled { get; init; }

    [Serialize]
    public bool Lock { get; init; }

}
#nullable enable