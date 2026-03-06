namespace MoreHttpApi.Shared.Specs;

[GeneratedCode("MoreHttpApi", "10.0.0")]
[ComponentSpec("Timberborn.SlotSystem.PatrollingSlotSpec")]
public record ParsedPatrollingSlotSpec(
    Single BaseMovementSpeed,
    Single MaxRandomDeviationOfMovementSpeed,
    string SlotKeyword,
    string Animation,
    Boolean WaterSlot
) : ParsedComponentSpec;