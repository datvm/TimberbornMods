namespace MoreHttpApi.Shared.Specs;

[GeneratedCode("MoreHttpApi", "10.0.0")]
[ComponentSpec("Timberborn.EnterableSystem.EnterableSpec")]
public record ParsedEnterableSpec(
    ParsedOperatingState OperatingState,
    Boolean LimitedCapacityFinished,
    Int32 CapacityFinished,
    Boolean LimitedCapacityUnfinished,
    Int32 CapacityUnfinished
) : ParsedComponentSpec;