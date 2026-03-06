namespace MoreHttpApi.Shared.Specs;

[GeneratedCode("MoreHttpApi", "10.0.0")]
[ComponentSpec("Timberborn.WindSystem.WindRotatorSpec")]
public record ParsedWindRotatorSpec(
    string TransformName,
    HttpSerializableFloats RotationAxis,
    Single RotationSpeed
) : ParsedComponentSpec;