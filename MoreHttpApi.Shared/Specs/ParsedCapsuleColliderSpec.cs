namespace MoreHttpApi.Shared.Specs;

[GeneratedCode("MoreHttpApi", "10.0.0")]
[ComponentSpec("Timberborn.UnityEngineSpecs.CapsuleColliderSpec")]
public record ParsedCapsuleColliderSpec(
    HttpSerializableFloats Center,
    Single Radius,
    Single Height,
    ParsedAxis Axis
) : ParsedComponentSpec;