namespace MoreHttpApi.Shared.Specs;

[GeneratedCode("MoreHttpApi", "10.0.0")]
[ComponentSpec("Timberborn.Goods.GoodGroupSpec")]
public record ParsedGoodGroupSpec(
    string Id,
    Int32 Order,
    string DisplayName,
    string Icon,
    Boolean SingleResourceGroup,
    string DisplayNameLocKey
) : ParsedComponentSpec, IComponentSpecWithId;