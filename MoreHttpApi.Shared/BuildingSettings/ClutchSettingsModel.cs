namespace MoreHttpApi.Shared.BuildingSettings;

public record ClutchSettingsModel(HttpClutchMode Value) : ValueSettingModel<HttpClutchMode>(Value);

public enum HttpClutchMode
{
    Engaged,
    Disengaged,
    Automated
}
