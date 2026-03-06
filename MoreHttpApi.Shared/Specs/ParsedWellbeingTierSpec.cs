namespace MoreHttpApi.Shared.Specs;

[GeneratedCode("MoreHttpApi", "10.0.0")]
[ComponentSpec("Timberborn.Wellbeing.WellbeingTierSpec")]
public record ParsedWellbeingTierSpec(
    string CharacterType,
    string BonusId,
    ParsedWellbeingTierBonusSpec[] Bonuses,
    Int32 WellbeingThreshold,
    Single MultiplierIncrement
) : ParsedComponentSpec;