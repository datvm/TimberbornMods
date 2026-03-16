namespace MoreHttpApi.Shared.Specs;

[GeneratedCode("MoreHttpApi", "10.0.0")]
[ComponentSpec("Timberborn.BonusSystem.BonusTypeSpec")]
public record ParsedBonusTypeSpec(
    string Id,
    string DisplayName,
    Single MinimumValue,
    Single MaximumValue,
    string Icon,
    string LocKey
) : ParsedComponentSpec, IComponentSpecWithId;