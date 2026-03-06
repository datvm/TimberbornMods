namespace MoreHttpApi.Shared.Specs;

[GeneratedCode("MoreHttpApi", "10.0.0")]
[ComponentSpec("Timberborn.AutomationUI.TransmitterPickerColorsSpec")]
public record ParsedTransmitterPickerColorsSpec(
    HttpSerializableFloats TransmitterColor,
    HttpSerializableFloats UnfinishedTransmitterColor,
    HttpSerializableFloats HoveredTransmitterColor
) : ParsedComponentSpec;