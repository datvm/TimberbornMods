namespace MoreHttpApi.Shared.Specs;

[GeneratedCode("MoreHttpApi", "10.0.0")]
[ComponentSpec("Timberborn.WindSystem.WindServiceSpec")]
public record ParsedWindServiceSpec(
    Single MinWindTimeInHours,
    Single MaxWindTimeInHours,
    Single MinWindStrength,
    Single MaxWindStrength
) : ParsedComponentSpec;