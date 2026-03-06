namespace MoreHttpApi.Shared.Specs;

[GeneratedCode("MoreHttpApi", "10.0.0")]
[ComponentSpec("Timberborn.Reproduction.BreedingPodSpec")]
public record ParsedBreedingPodSpec(
    string EmbryoName,
    Single CycleLengthInDays,
    Int32 CyclesUntilFullyGrown,
    Int32 CyclesCapacity,
    ParsedGoodAmountSpec[] NutrientsPerCycle,
    Boolean SpawnAdults
) : ParsedComponentSpec;