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
    public Sprite? Icon { get; init; }

    [Serialize]
    public ImmutableArray<float> Parameters { get; init; } = [];

    [Serialize]
    public ImmutableArray<TimberQuestStepSpec> Steps { get; init; } = [];

    [Serialize]
    public ImmutableArray<TimberQuestRewardSpec> Rewards { get; init; } = [];
    
    public bool Initialized { get; internal set; }
    public bool HasReward => Rewards.Length > 0;

    public override string ToString()
    {
        return string.IsNullOrEmpty(Name) ? $"Quest Id {Id}" : $"Quest Id {Id} ({Name})";

    }
}
