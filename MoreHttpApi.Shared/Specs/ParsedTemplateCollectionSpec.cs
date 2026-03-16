namespace MoreHttpApi.Shared.Specs;

[GeneratedCode("MoreHttpApi", "10.0.0")]
[ComponentSpec("Timberborn.TemplateCollectionSystem.TemplateCollectionSpec")]
public record ParsedTemplateCollectionSpec(
    string CollectionId,
    string[] Blueprints
) : ParsedComponentSpec, IComponentSpecWithId
{
    public string Id => CollectionId;
}