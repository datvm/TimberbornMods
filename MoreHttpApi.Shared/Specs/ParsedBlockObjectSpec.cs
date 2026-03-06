namespace MoreHttpApi.Shared.Specs;

[GeneratedCode("MoreHttpApi", "10.0.0")]
[ComponentSpec("Timberborn.BlockSystem.BlockObjectSpec")]
public record ParsedBlockObjectSpec(
    HttpSerializableInts Size,
    ParsedBlockSpec[] Blocks,
    ParsedEntranceBlockSpec Entrance,
    Int32 BaseZ,
    Boolean Overridable,
    Boolean Flippable
) : ParsedComponentSpec;