namespace MoreHttpApi.Shared.Specs;

[GeneratedCode("MoreHttpApi", "10.0.0")]
[ComponentSpec("Timberborn.StockpileVisualization.StockpilePlaneVisualization")]
public record ParsedStockpilePlaneVisualization(
    HttpSerializableFloats CenterOffset,
    HttpSerializableFloats MovementRange,
    string GoodVisualizationId,
    string GoodVisualizationVariant
) : ParsedComponentSpec;