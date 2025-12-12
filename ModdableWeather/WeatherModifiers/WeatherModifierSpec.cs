namespace ModdableWeather.WeatherModifiers;

#nullable disable
public record WeatherModifierSpec : ComponentSpec
{

    [Serialize]
    public string Id { get; init; }

    [Serialize]
    public string DisplayLoc { get; init; }

    [Serialize(nameof(DisplayLoc))]
    public LocalizedText Display { get; init; }

    [Serialize]
    public ImmutableArray<WeatherModifierCompatibility> Compatibilities { get; init; }

}

public record WeatherModifierCompatibility
{
    [Serialize]
    public string WeatherId { get; init; }

    [Serialize]
    public int Chance { get; init; }

    [Serialize]
    public int StartCycle { get; init; }

    [Serialize]
    public bool UserConfigurable { get; init; } = true;
}

#nullable enable