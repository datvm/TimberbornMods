namespace MoreHttpApi.Shared.Specs;

[GeneratedCode("MoreHttpApi", "10.0.0")]
[ComponentSpec("Timberborn.UnityEngineSpecs.AnimationKeyframeSpec")]
public record ParsedAnimationKeyframeSpec(
    Single Time,
    Single Value,
    Single InTangent,
    Single OutTangent,
    Int32 WeightedMode,
    Single InWeight,
    Single OutWeight
) : ParsedComponentSpec;