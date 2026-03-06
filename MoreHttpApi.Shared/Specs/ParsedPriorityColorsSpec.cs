namespace MoreHttpApi.Shared.Specs;

[GeneratedCode("MoreHttpApi", "10.0.0")]
[ComponentSpec("Timberborn.PrioritySystemUI.PriorityColorsSpec")]
public record ParsedPriorityColorsSpec(
    HttpSerializableFloats HighlightVeryLow,
    HttpSerializableFloats HighlightLow,
    HttpSerializableFloats HighlightNormal,
    HttpSerializableFloats HighlightHigh,
    HttpSerializableFloats HighlightVeryHigh,
    HttpSerializableFloats ButtonVeryLow,
    HttpSerializableFloats ButtonLow,
    HttpSerializableFloats ButtonNormal,
    HttpSerializableFloats ButtonHigh,
    HttpSerializableFloats ButtonVeryHigh
) : ParsedComponentSpec;