
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
    public string EffectKey { get; init; } = null!;
    public string Effect { get; internal set; } = null!;

    [Serialize(isOptional: true)]
    public ImmutableArray<float> Parameters { get; init; } 

    [Serialize]
    public int ScienceCost { get; init; }

    [Serialize(isOptional: true)]
    public int MaxSteps { get; init; }
    public bool HasSteps => MaxSteps > 0;

    [Serialize(isOptional: true)]
    public bool HasScalingCost { get; init; }

    [Serialize(isOptional: true)]
    public bool HasCustomUnlockCondition { get; init; }

    [Serialize(isOptional: true)]
    public string? ScalingCostKey { get; init; }
    public string? ScalingCostDisplay { get; internal set; }

    [Serialize(isOptional: true)]
    public string? RequiredId { get; init; }

    [Serialize(isOptional: true)]
    public Texture2D? Icon { get; init; }

    [Serialize(isOptional: true)]
    public int Order { get; init; }

    public override string ToString() => $"Project {Id}: {DisplayName}";

}
