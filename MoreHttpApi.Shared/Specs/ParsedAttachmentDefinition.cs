namespace MoreHttpApi.Shared.Specs;

[GeneratedCode("MoreHttpApi", "10.0.0")]
[ComponentSpec("Timberborn.TemplateAttachmentSystem.AttachmentDefinition")]
public record ParsedAttachmentDefinition(
    string AttachmentId,
    string Prefab,
    string Parent,
    HttpSerializableFloats Position,
    HttpSerializableFloats Rotation,
    HttpSerializableFloats Scale,
    Boolean CreateInstantly
) : ParsedComponentSpec;