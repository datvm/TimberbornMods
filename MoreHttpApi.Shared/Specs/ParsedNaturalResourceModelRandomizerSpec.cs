namespace MoreHttpApi.Shared.Specs;

[GeneratedCode("MoreHttpApi", "10.0.0")]
[ComponentSpec("Timberborn.NaturalResourcesModelSystem.NaturalResourceModelRandomizerSpec")]
public record ParsedNaturalResourceModelRandomizerSpec(
    Boolean ConstrainProportion,
    Single MinHeightScaleFactor,
    Single MaxHeightScaleFactor,
    Single MinWidthScaleFactor,
    Single MaxWidthScaleFactor,
    ParsedRandomizeRotationMode RandomizedRotation,
    Single MinRotation,
    Single MaxRotation
) : ParsedComponentSpec;