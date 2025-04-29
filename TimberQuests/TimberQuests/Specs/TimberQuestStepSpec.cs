namespace TimberQuests;

public record TimberQuestStepSpec : ComponentSpec
{

    [Serialize]
    public string NameKey { get; init; } = null!;
    public string Name { get; set; } = null!;

    [Serialize]
    public string? DescriptionKey { get; init; }
    public string? Description { get; set; }

    [Serialize]
    public ImmutableArray<float> Parameters { get; init; } = [];

    [Serialize]
    public bool Disabled { get; init; }

}
