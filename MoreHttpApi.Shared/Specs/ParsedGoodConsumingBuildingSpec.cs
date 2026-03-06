namespace MoreHttpApi.Shared.Specs;

[GeneratedCode("MoreHttpApi", "10.0.0")]
[ComponentSpec("Timberborn.GoodConsumingBuildingSystem.GoodConsumingBuildingSpec")]
public record ParsedGoodConsumingBuildingSpec(
    Int32 FullInventoryWorkHours,
    ParsedConsumedGoodSpec[] ConsumedGoods
) : ParsedComponentSpec;