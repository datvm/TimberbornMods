
namespace ScientificProjects;

public record ScientificProjectSpec : ComponentSpec
{

    [Serialize]
    public string Id { get; init; } = null!;

    [Serialize]
    public string GroupId { get; init; } = null!;

    [Serialize]
    public string NameKey { get; init; } = null!;
    public string DisplayName { get; internal set; } = null!;

    [Serialize]
    public string? LoreKey { get; init; }
    public string? Lore { get; internal set; }

    [Serialize]
    public ImmutableArray<string> Factions
    {
        get;
        init
        {
            field = value == default ? [] : value;
        }
    } = [];

    [Serialize]
    public string EffectKey { get; init; } = null!;
    public string Effect { get; internal set; } = null!;

    [Serialize]
    public ImmutableArray<float> Parameters { get; init; }

    [Serialize]
    public int ScienceCost { get; init; }

    [Serialize]
    public int MaxSteps { get; init; }
    public bool HasSteps => MaxSteps > 0;

    [Serialize]
    public bool HasScalingCost { get; init; }

    [Serialize]
    public bool HasCustomUnlockCondition { get; init; }

    [Serialize]
    public string? ScalingCostKey { get; init; }
    public string? ScalingCostDisplay { get; internal set; }

    [Serialize]
    public string? RequiredId { get; init; }

    [Serialize]
    public Texture2D? Icon { get; init; }

    [Serialize]
    public int Order { get; init; }

    [Serialize]
    public bool NeedReload { get; init; }

    public override string ToString() => $"Project {Id}: {DisplayName}";

}
