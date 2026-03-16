namespace MoreHttpApi.Shared.Specs;

[GeneratedCode("MoreHttpApi", "10.0.0")]
[ComponentSpec("Timberborn.KeyBindingSystem.KeyBindingGroupSpec")]
public record ParsedKeyBindingGroupSpec(
    string Id,
    Int32 Order,
    string DisplayName,
    string LocKey
) : ParsedComponentSpec, IComponentSpecWithId;