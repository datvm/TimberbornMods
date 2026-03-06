namespace MoreHttpApi.Shared.Specs;

[GeneratedCode("MoreHttpApi", "10.0.0")]
[ComponentSpec("Timberborn.UnityEngineSpecs.TransformSpec")]
public record ParsedTransformSpec(
    HttpSerializableFloats Position,
    HttpSerializableFloats Rotation,
    HttpSerializableFloats Scale
) : ParsedComponentSpec;