namespace MoreHttpApi.Shared.Specs;

[GeneratedCode("MoreHttpApi", "10.0.0")]
[ComponentSpec("Timberborn.CameraSystem.CameraServiceSpec")]
public record ParsedCameraServiceSpec(
    Single HorizontalAngle,
    Single VerticalAngle,
    ParsedFloatLimitsSpec VerticalAngleLimits,
    Single ZoomLevel,
    Single ZoomSpeed,
    Single ZoomBase,
    Single BaseDistance,
    ParsedFloatLimitsSpec DefaultZoomLimits,
    ParsedFloatLimitsSpec UnlockedZoomLimits,
    ParsedFloatLimitsSpec MapEditorZoomLimits,
    ParsedFloatLimitsSpec FreeModeZoomLimits,
    Single MapMargin,
    Single FreeModeMapMargin
) : ParsedComponentSpec;