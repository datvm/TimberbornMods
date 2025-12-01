
namespace ScientificProjects;

public record ScientificProjectSpec : ComponentSpec
{

    [Serialize]
    public string Id { get; init; } = null!;

    [Serialize]
    public string GroupId { get; init; } = null!;

    [Serialize]
    public string NameKey { get; init; } = null!;
    public string DisplayName { get; set; } = null!;

    [Serialize]
    public string? LoreKey { get; init; }
    public string? Lore { get; set; }

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
    public string Effect { get; set; } = null!;

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
    public string? ScalingCostDisplay { get; set; }

    [Serialize]
    public string? RequiredId { get; init; }

    [Serialize]
    public AssetRef<Texture2D>? Icon { get; init; }

    [Serialize]
    public int Order { get; init; }

    [Serialize]
    public bool NeedReload { get; init; }

    public bool NeedUnlock => MaxSteps == 0;
    public override string ToString() => $"Project {Id}: {DisplayName}";

    public float GetEffect(int parameterIndex, int level)
        => Parameters[parameterIndex] * level;

}
