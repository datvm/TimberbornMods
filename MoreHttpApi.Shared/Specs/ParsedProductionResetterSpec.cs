namespace MoreHttpApi.Shared.Specs;

[GeneratedCode("MoreHttpApi", "10.0.0")]
[ComponentSpec("Timberborn.Workshops.ProductionResetterSpec")]
public record ParsedProductionResetterSpec(
    Single HoursToResetProgress,
    string StatusLocKey,
    string AlertLocKey,
    string StatusIcon
) : ParsedComponentSpec;