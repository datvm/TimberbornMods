namespace MoreHttpApi.Shared.Specs;

[GeneratedCode("MoreHttpApi", "10.0.0")]
[ComponentSpec("Timberborn.NeedSpecs.CriticalNeedSpec")]
public record ParsedCriticalNeedSpec(
    ParsedCriticalNeedType CriticalNeedType,
    string SpriteName,
    string Description,
    string DescriptionShort,
    string DescriptionLocKey,
    string DescriptionShortLocKey
) : ParsedComponentSpec;