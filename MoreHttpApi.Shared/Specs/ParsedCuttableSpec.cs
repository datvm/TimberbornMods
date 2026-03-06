namespace MoreHttpApi.Shared.Specs;

[GeneratedCode("MoreHttpApi", "10.0.0")]
[ComponentSpec("Timberborn.Cutting.CuttableSpec")]
public record ParsedCuttableSpec(
    Boolean RemoveOnCut,
    string LeftoverModelName,
    ParsedYielderSpec Yielder
) : ParsedComponentSpec;