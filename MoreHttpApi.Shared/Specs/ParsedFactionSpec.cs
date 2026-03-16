namespace MoreHttpApi.Shared.Specs;

[GeneratedCode("MoreHttpApi", "10.0.0")]
[ComponentSpec("Timberborn.FactionSystem.FactionSpec")]
public record ParsedFactionSpec(
    string Id,
    Int32 Order,
    string DisplayName,
    string Description,
    string Avatar,
    string ChildAvatar,
    string BotAvatar,
    string ContaminatedAdultAvatar,
    string ContaminatedChildAvatar,
    string Logo,
    string NewGameFullAvatar,
    string[] Textures,
    string[] ChildTextures,
    string[] MaterialCollectionIds,
    string[] TemplateCollectionIds,
    string PathMaterial,
    string BaseWoodMaterial,
    string[] NeedCollectionIds,
    string[] GoodCollectionIds,
    ParsedBlueprintModifierSpec[] BlueprintModifiers,
    string StartingBuildingId,
    string SoundId,
    string GameOverMessage,
    string GameOverFlavor,
    string DisplayNameLocKey,
    string DescriptionLocKey,
    string GameOverMessageLocKey,
    string GameOverFlavorLocKey
) : ParsedComponentSpec, IComponentSpecWithId;