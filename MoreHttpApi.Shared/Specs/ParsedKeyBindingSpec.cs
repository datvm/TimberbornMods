namespace MoreHttpApi.Shared.Specs;

[GeneratedCode("MoreHttpApi", "10.0.0")]
[ComponentSpec("Timberborn.KeyBindingSystem.KeyBindingSpec")]
public record ParsedKeyBindingSpec(
    string Id,
    string GroupId,
    string LocKey,
    Int32 Order,
    Boolean AllowOtherModifiers,
    Boolean DevModeOnly
) : ParsedComponentSpec;