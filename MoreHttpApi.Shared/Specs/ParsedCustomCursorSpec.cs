namespace MoreHttpApi.Shared.Specs;

[GeneratedCode("MoreHttpApi", "10.0.0")]
[ComponentSpec("Timberborn.InputSystem.CustomCursorSpec")]
public record ParsedCustomCursorSpec(
    string Id,
    string WindowsCursor,
    string MacOsCursor,
    HttpSerializableFloats Hotspot,
    HttpSerializableFloats WindowsCursorOffset,
    HttpSerializableFloats MacOsCursorOffset
) : ParsedComponentSpec;