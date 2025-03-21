global using Timberborn.BlueprintSystem;

namespace ModdablePrefab;

public record PrefabModderSpec : BasePrefabModSpec
{

    [Serialize]
    public string ValuePath { get; init; } = null!;

    [Serialize]
    public string NewValue { get; init; } = null!;
    public string NormalizedNewValue { get; internal set; } = null!;

    [Serialize(true)]
    public bool AppendArray { get; init; }

}
