namespace MoreHttpApi.Shared.Specs;

[GeneratedCode("MoreHttpApi", "10.0.0")]
[ComponentSpec("Timberborn.AutomationBuildings.FlowSensorSpec")]
public record ParsedFlowSensorSpec(
    HttpSerializableInts SensorCoordinates,
    Single MaxThreshold
) : ParsedComponentSpec;