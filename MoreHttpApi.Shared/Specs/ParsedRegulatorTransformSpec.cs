namespace MoreHttpApi.Shared.Specs;

[GeneratedCode("MoreHttpApi", "10.0.0")]
[ComponentSpec("Timberborn.WaterSourceSystem.RegulatorTransformSpec")]
public record ParsedRegulatorTransformSpec(
    string TransformName,
    HttpSerializableFloats TargetOffset,
    HttpSerializableFloats TargetRotation
) : ParsedComponentSpec;