namespace MoreHttpApi.Shared.Specs;

[GeneratedCode("MoreHttpApi", "10.0.0")]
[ComponentSpec("Timberborn.MapStateSystem.MapSizeSpec")]
public record ParsedMapSizeSpec(
    HttpSerializableInts DefaultMapSize,
    Int32 MinMapSize,
    Int32 MaxMapSize,
    Int32 MaxMapEditorTerrainHeight,
    Int32 MaxGameTerrainHeight,
    Int32 MaxHeightAboveTerrain
) : ParsedComponentSpec;