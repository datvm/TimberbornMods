namespace MoreHttpApi.Shared.Specs;

[GeneratedCode("MoreHttpApi", "10.0.0")]
[ComponentSpec("Timberborn.BonusSystem.BonusSpec")]
public record ParsedBonusSpec(
    string Id,
    Single MultiplierDelta
) : ParsedComponentSpec, IComponentSpecWithId;