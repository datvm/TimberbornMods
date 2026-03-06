namespace MoreHttpApi.Shared.Specs;

[GeneratedCode("MoreHttpApi", "10.0.0")]
[ComponentSpec("Timberborn.SoilBarrierSystem.SoilBarrierSpec")]
public record ParsedSoilBarrierSpec(
    Boolean BlockAboveMoisture,
    Boolean BlockFullMoisture,
    Boolean BlockContamination
) : ParsedComponentSpec;