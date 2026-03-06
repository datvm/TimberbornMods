namespace MoreHttpApi.Shared.Specs;

[GeneratedCode("MoreHttpApi", "10.0.0")]
[ComponentSpec("Timberborn.StockpileVisualization.StockpileGoodPileVisualizerSpec")]
public record ParsedStockpileGoodPileVisualizerSpec(
    HttpSerializableFloats CenterOffset,
    string[] GoodPileVisualizations
) : ParsedComponentSpec;