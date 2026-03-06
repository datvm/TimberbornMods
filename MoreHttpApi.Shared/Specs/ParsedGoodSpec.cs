namespace MoreHttpApi.Shared.Specs;

[GeneratedCode("MoreHttpApi", "10.0.0")]
[ComponentSpec("Timberborn.Goods.GoodSpec")]
public record ParsedGoodSpec(
    string Id,
    string[] BackwardCompatibleIds,
    string DisplayName,
    string PluralDisplayName,
    ParsedInstantEffectSpec[] ConsumptionEffects,
    string GoodType,
    string StockpileVisualization,
    ParsedVisibleContainer VisibleContainer,
    HttpSerializableFloats ContainerColor,
    string ContainerMaterial,
    string CarryingAnimation,
    Int32 Weight,
    string GoodGroupId,
    Int32 GoodOrder,
    string Icon,
    Boolean ForceImport,
    string DisplayNameLocKey,
    string PluralDisplayNameLocKey
) : ParsedComponentSpec;