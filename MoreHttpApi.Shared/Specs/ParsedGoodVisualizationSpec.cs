namespace MoreHttpApi.Shared.Specs;

[GeneratedCode("MoreHttpApi", "10.0.0")]
[ComponentSpec("Timberborn.StockpileVisualization.GoodVisualizationSpec")]
public record ParsedGoodVisualizationSpec(
    string Id,
    string Variant,
    HttpSerializableFloats Offset,
    Single LimitingAmount,
    string PrimaryMesh,
    string SecondaryMesh,
    string Material,
    Single NonLinearity
) : ParsedComponentSpec, IComponentSpecWithId;