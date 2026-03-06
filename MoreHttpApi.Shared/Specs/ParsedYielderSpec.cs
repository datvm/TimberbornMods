namespace MoreHttpApi.Shared.Specs;

[GeneratedCode("MoreHttpApi", "10.0.0")]
[ComponentSpec("Timberborn.Yielding.YielderSpec")]
public record ParsedYielderSpec(
    string YielderComponentName,
    ParsedGoodAmountSpec Yield,
    Single RemovalTimeInHours,
    string ResourceGroup
) : ParsedComponentSpec;