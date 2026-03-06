namespace MoreHttpApi.Shared.Specs;

[GeneratedCode("MoreHttpApi", "10.0.0")]
[ComponentSpec("Timberborn.UnityEngineSpecs.AnimationCurveSpec")]
public record ParsedAnimationCurveSpec(
    ParsedAnimationKeyframeSpec[] Keys,
    ParsedWrapMode PreWrapMode,
    ParsedWrapMode PostWrapMode
) : ParsedComponentSpec;