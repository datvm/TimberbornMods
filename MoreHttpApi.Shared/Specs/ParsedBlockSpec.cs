namespace MoreHttpApi.Shared.Specs;

[GeneratedCode("MoreHttpApi", "10.0.0")]
[ComponentSpec("Timberborn.BlockSystem.BlockSpec")]
public record ParsedBlockSpec(
    ParsedMatterBelow MatterBelow,
    ParsedBlockOccupations Occupations,
    ParsedBlockStackable Stackable,
    Boolean OccupyAllBelow,
    Boolean Underground
) : ParsedComponentSpec;