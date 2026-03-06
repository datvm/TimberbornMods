namespace MoreHttpApi.Shared.Specs;

[GeneratedCode("MoreHttpApi", "10.0.0")]
[ComponentSpec("Timberborn.TemplateSystem.TemplateSpec")]
public record ParsedTemplateSpec(
    string TemplateName,
    string[] BackwardCompatibleTemplateNames,
    string RequiredFeatureToggle,
    string DisablingFeatureToggle
) : ParsedComponentSpec;