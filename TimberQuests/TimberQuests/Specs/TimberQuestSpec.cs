namespace TimberQuests;

public record TimberQuestSpec : ComponentSpec
{

    [Serialize]
    public string Id { get; init; } = null!;

    [Serialize]
    public string NameKey { get; init; } = null!;
    public string Name { get; set; } = null!;

    [Serialize]
    public string? DescriptionKey { get; init; }
    public string? Description { get; set; }

    [Serialize]
    public string? RewardsKey { get; init; }
    public string? Rewards { get; set; }

    [Serialize]
    public Texture2D? Icon { get; init; }

    [Serialize]
    public ImmutableArray<float> Parameters { get; init; } = [];

    [Serialize]
    public ImmutableArray<TimberQuestStepSpec> Steps { get; init; } = [];

    public override string ToString()
    {
        return string.IsNullOrEmpty(Name) ? $"Quest Id {Id}" : $"Quest Id {Id} ({Name})";

    }
}
