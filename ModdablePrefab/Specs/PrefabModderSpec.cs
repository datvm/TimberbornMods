global using Timberborn.BlueprintSystem;

namespace ModdablePrefab;

public record PrefabModderSpec : ComponentSpec
{

    [Serialize]
    public string ComponentType { get; init; } = null!;

    [Serialize(true)]
    public ImmutableArray<string> PrefabNames { get; init; }

    [Serialize]
    public string ValuePath { get; init; } = null!;

    [Serialize]
    public string NewValue { get; init; } = null!;

    [Serialize(true)]
    public bool AppendArray { get; init; }

}
